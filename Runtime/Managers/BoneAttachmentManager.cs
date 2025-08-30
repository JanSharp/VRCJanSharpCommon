using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [DefaultExecutionOrder(-1000000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonScript("00766e043c297fab8ab20626ec22ab6a")] // Runtime/Prefabs/BoneAttachmentManager.prefab
    public class BoneAttachmentManager : UdonSharpBehaviour
    {
        [SerializeField] private GameObject attachmentPrefab;
        [SerializeField] private Transform proximityHelper;
        private Transform[] unusedPrefabInstances = new Transform[ArrList.MinCapacity];
        private int unusedPrefabInstancesCount = 0;

        private VRCPlayerApi localPlayer;
        private Vector3 localPlayerPosition;
        private int localPlayerId;

        // Requirements:
        // - Fast registration
        // - Fast de-registration
        // - Very fast iteration
        //   - Which means we have to use arrays, no DataDictionary, no DataList, no object[] as a "class"

        private int[] nearAttachedPlayerIds = new int[ArrList.MinCapacity];
        private VRCPlayerApi[] nearAttachedPlayers = new VRCPlayerApi[ArrList.MinCapacity];
        private int[] nearAttachedBones = new int[ArrList.MinCapacity];
        private Transform[] nearAttachedTransforms = new Transform[ArrList.MinCapacity];
        private int nearAttachedCount = 0;
        private int nearIncrementalIndex = 0;

        private int[] farAttachedPlayerIds = new int[ArrList.MinCapacity];
        private VRCPlayerApi[] farAttachedPlayers = new VRCPlayerApi[ArrList.MinCapacity];
        private int[] farAttachedBones = new int[ArrList.MinCapacity];
        private Transform[] farAttachedTransforms = new Transform[ArrList.MinCapacity];
        private int farAttachedCount = 0;
        private int farIncrementalIndex = 0;

        private int[] localBones = new int[ArrList.MinCapacity];
        private Transform[] localBoneTransforms = new Transform[ArrList.MinCapacity];
        private int localBonesCount = 0;

        private int[] localTrackingTypes = new int[ArrList.MinCapacity];
        private Transform[] localTrackingTransforms = new Transform[ArrList.MinCapacity];
        private int localTrackingCount = 0;

        private const float NearDistanceThreshold = 32f;

#if JAN_SHARP_COMMON_STOPWATCH
        [HideInInspector][SerializeField][SingletonReference] private QuickDebugUI qd;
        private System.Diagnostics.Stopwatch updateSw = new System.Diagnostics.Stopwatch();
        private object[] updateContainer;
#endif

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            localPlayerId = localPlayer.playerId;
#if JAN_SHARP_COMMON_STOPWATCH
            updateContainer = StopwatchUtil.CreateDataContainer();
#endif
        }

        private void Update()
        {
#if JAN_SHARP_COMMON_STOPWATCH
            updateSw.Reset();
            updateSw.Start();
#endif
            // Local player.
            for (int i = 0; i < localBonesCount; i++)
            {
                HumanBodyBones bone = (HumanBodyBones)localBones[i];
                Vector3 bonePosition = localPlayer.GetBonePosition(bone);
                if (bonePosition != Vector3.zero)
                    localBoneTransforms[i].SetPositionAndRotation(bonePosition, localPlayer.GetBoneRotation(bone));
            }
            for (int i = 0; i < localTrackingCount; i++)
            {
                VRCPlayerApi.TrackingData data = localPlayer.GetTrackingData((VRCPlayerApi.TrackingDataType)localTrackingTypes[i]);
                localTrackingTransforms[i].SetPositionAndRotation(data.position, data.rotation);
            }

            // Remote players.
            localPlayerPosition = localPlayer.GetPosition();
            DistanceCheckNearIncremental();
            UpdateNear();
            UpdateFarIncremental();

#if JAN_SHARP_COMMON_STOPWATCH
            updateSw.Stop();
            qd.ShowForOneFrame(this, "Update MS", StopwatchUtil.FormatAvgMinMax(updateSw, updateContainer));
#endif
        }

        private void FixedUpdate()
        {
            proximityHelper.position = localPlayer.GetPosition();
        }

        public void OnPlayerGettingClose(VRCPlayerApi player)
        {
            int playerId = player.playerId;
            if (playerId == localPlayerId)
                return;
            for (int i = farAttachedCount - 1; i >= 0; i--)
            {
                if (farAttachedPlayerIds[i] != playerId)
                    continue;
                if (UpdateFarObject(i) && i < farIncrementalIndex)
                    farIncrementalIndex--;
            }
        }

        private void DistanceCheckNearIncremental()
        {
            if (nearAttachedCount == 0)
                return;
            nearIncrementalIndex %= nearAttachedCount;
            VRCPlayerApi player = nearAttachedPlayers[nearIncrementalIndex];
            if (!Utilities.IsValid(player)) // Ew, but this is faster than 'player == null || !player.IsValid()'.
                return;
            HumanBodyBones bone = (HumanBodyBones)nearAttachedBones[nearIncrementalIndex];
            Vector3 bonePosition = player.GetBonePosition(bone);
            // When zero, the bone does not exist and tracking it gets disabled. However it does not get
            // unregistered, so just keep it in the incremental far update loop.
            if (bonePosition == Vector3.zero || !IsNear(bonePosition))
                MoveFromNearToFar(nearIncrementalIndex);
            nearIncrementalIndex++;
        }

        private void UpdateNear()
        {
            for (int i = 0; i < nearAttachedCount; i++)
            {
                VRCPlayerApi player = nearAttachedPlayers[i];
                if (!Utilities.IsValid(player)) // Ew, but this is faster than 'player == null || !player.IsValid()'.
                    continue;
                HumanBodyBones bone = (HumanBodyBones)nearAttachedBones[i];
                Vector3 bonePosition = player.GetBonePosition(bone);
                if (bonePosition != Vector3.zero)
                    nearAttachedTransforms[i].SetPositionAndRotation(bonePosition, player.GetBoneRotation(bone));
            }
        }

        private bool UpdateFarObject(int index)
        {
            VRCPlayerApi player = farAttachedPlayers[index];
            if (!Utilities.IsValid(player)) // Ew, but this is faster than 'player == null || !player.IsValid()'.
                return false;
            HumanBodyBones bone = (HumanBodyBones)farAttachedBones[index];
            Vector3 bonePosition = player.GetBonePosition(bone);
            if (bonePosition == Vector3.zero)
                return false;
            farAttachedTransforms[index].SetPositionAndRotation(bonePosition, player.GetBoneRotation(bone));
            bool isNear = IsNear(bonePosition);
            if (isNear)
                MoveFromFarToNear(index);
            return isNear;
        }

        private void UpdateFarIncremental()
        {
            if (farAttachedCount == 0)
                return;
            farIncrementalIndex %= farAttachedCount;
            UpdateFarObject(farIncrementalIndex);
            farIncrementalIndex++;
        }

        private bool IsNear(Vector3 bonePosition) => Vector3.Distance(localPlayerPosition, bonePosition) <= NearDistanceThreshold;

        private void MoveFromNearToFar(int nearIndex)
        {
            int farIndex = farAttachedCount;
            IncrementFarAttachedCount();
            farAttachedPlayerIds[farIndex] = nearAttachedPlayerIds[nearIndex];
            farAttachedPlayers[farIndex] = nearAttachedPlayers[nearIndex];
            farAttachedBones[farIndex] = nearAttachedBones[nearIndex];
            farAttachedTransforms[farIndex] = nearAttachedTransforms[nearIndex];
            RemoveFromNearArrays(nearIndex);
        }

        private void MoveFromFarToNear(int farIndex)
        {
            int nearIndex = nearAttachedCount;
            IncrementNearAttachedCount();
            nearAttachedPlayerIds[nearIndex] = farAttachedPlayerIds[farIndex];
            nearAttachedPlayers[nearIndex] = farAttachedPlayers[farIndex];
            nearAttachedBones[nearIndex] = farAttachedBones[farIndex];
            nearAttachedTransforms[nearIndex] = farAttachedTransforms[farIndex];
            RemoveFromFarArrays(farIndex);
        }

        private void RemoveFromNearArrays(int nearIndex)
        {
            // Move top of the arrays down to the removed index.
            nearAttachedCount--;
            if (nearIndex == nearAttachedCount) // Just an optimization.
                return;
            nearAttachedPlayerIds[nearIndex] = nearAttachedPlayerIds[nearAttachedCount];
            nearAttachedPlayers[nearIndex] = nearAttachedPlayers[nearAttachedCount];
            nearAttachedBones[nearIndex] = nearAttachedBones[nearAttachedCount];
            nearAttachedTransforms[nearIndex] = nearAttachedTransforms[nearAttachedCount];
        }

        private void RemoveFromFarArrays(int farIndex)
        {
            // Move top of the arrays down to the removed index.
            farAttachedCount--;
            if (farIndex == farAttachedCount) // Just an optimization.
                return;
            farAttachedPlayerIds[farIndex] = farAttachedPlayerIds[farAttachedCount];
            farAttachedPlayers[farIndex] = farAttachedPlayers[farAttachedCount];
            farAttachedBones[farIndex] = farAttachedBones[farAttachedCount];
            farAttachedTransforms[farIndex] = farAttachedTransforms[farAttachedCount];
        }

        private void RemoveFromLocalBonesArrays(int index)
        {
            // Move top of the arrays down to the removed index.
            localBonesCount--;
            if (index == localBonesCount) // Just an optimization.
                return;
            localBones[index] = localBones[localBonesCount];
            localBoneTransforms[index] = localBoneTransforms[localBonesCount];
        }

        private void RemoveFromLocalTrackingArrays(int index)
        {
            // Move top of the arrays down to the removed index.
            localTrackingCount--;
            if (index == localTrackingCount) // Just an optimization.
                return;
            localTrackingTypes[index] = localTrackingTypes[localTrackingCount];
            localTrackingTransforms[index] = localTrackingTransforms[localTrackingCount];
        }

        private void IncrementNearAttachedCount()
        {
            nearAttachedCount++;
            ArrList.EnsureCapacity(ref nearAttachedPlayerIds, nearAttachedCount);
            ArrList.EnsureCapacity(ref nearAttachedPlayers, nearAttachedCount);
            ArrList.EnsureCapacity(ref nearAttachedBones, nearAttachedCount);
            ArrList.EnsureCapacity(ref nearAttachedTransforms, nearAttachedCount);
        }

        private void IncrementFarAttachedCount()
        {
            farAttachedCount++;
            ArrList.EnsureCapacity(ref farAttachedPlayerIds, farAttachedCount);
            ArrList.EnsureCapacity(ref farAttachedPlayers, farAttachedCount);
            ArrList.EnsureCapacity(ref farAttachedBones, farAttachedCount);
            ArrList.EnsureCapacity(ref farAttachedTransforms, farAttachedCount);
        }

        private void IncrementLocalBonesCount()
        {
            localBonesCount++;
            ArrList.EnsureCapacity(ref localBones, localBonesCount);
            ArrList.EnsureCapacity(ref localBoneTransforms, localBonesCount);
        }

        private void IncrementLocalTrackingCount()
        {
            localTrackingCount++;
            ArrList.EnsureCapacity(ref localTrackingTypes, localTrackingCount);
            ArrList.EnsureCapacity(ref localTrackingTransforms, localTrackingCount);
        }

        private static int IndexOfInternal<T1, T2>(T1[] arrayOne, T2[] arrayTwo, int listCount, T1 valueOne, T2 valueTwo)
        {
            int index = -1;
            while (true)
            {
                index = System.Array.IndexOf(arrayOne, valueOne, index + 1, listCount);
                if (index == -1 || arrayTwo[index].Equals(valueTwo))
                    return index;
            }
        }
        private int IndexOfInNear(int playerId, int bone)
            => IndexOfInternal(nearAttachedPlayerIds, nearAttachedBones, nearAttachedCount, playerId, bone);
        private int IndexOfInFar(int playerId, int bone)
            => IndexOfInternal(farAttachedPlayerIds, farAttachedBones, farAttachedCount, playerId, bone);

        private int IndexOfInLocalBones(int bone) => System.Array.IndexOf(localBones, bone, 0, localBonesCount);
        private int IndexOfInLocalTracking(int trackingType) => System.Array.IndexOf(localTrackingTypes, trackingType, 0, localTrackingCount);

        private Transform GetAttachmentTransform() => unusedPrefabInstancesCount > 0
            ? ArrList.RemoveAt(ref unusedPrefabInstances, ref unusedPrefabInstancesCount, unusedPrefabInstancesCount - 1)
            : Instantiate(attachmentPrefab, this.transform).transform;

        private void AttachToBoneTransform(Transform boneTransform, VRCPlayerApi player, HumanBodyBones bone, Transform toAttach)
        {
            boneTransform.SetPositionAndRotation(player.GetBonePosition(bone), player.GetBoneRotation(bone));
            toAttach.SetParent(boneTransform); // After updating the bone transform to make it a clean transition.
        }

        private void AttachToCurrentTrackingData(Transform trackingTransform, VRCPlayerApi player, VRCPlayerApi.TrackingDataType trackingType, Transform toAttach)
        {
            VRCPlayerApi.TrackingData data = player.GetTrackingData(trackingType);
            trackingTransform.SetPositionAndRotation(data.position, data.rotation);
            toAttach.SetParent(trackingTransform); // After updating the tracking transform to make it a clean transition.
        }

        private void AttachToLocalPlayerBone(HumanBodyBones bone, int boneValue, Transform toAttach)
        {
            int index = IndexOfInLocalBones(boneValue);
            if (index != -1)
            {
                AttachToBoneTransform(localBoneTransforms[index], localPlayer, bone, toAttach);
                return;
            }

            Transform boneTransform = GetAttachmentTransform();
            AttachToBoneTransform(boneTransform, localPlayer, bone, toAttach);

            index = localBonesCount;
            IncrementLocalBonesCount();
            localBones[index] = boneValue;
            localBoneTransforms[index] = boneTransform;
        }

        public void AttachToLocalTrackingData(VRCPlayerApi.TrackingDataType trackingType, Transform toAttach)
        {
            int trackingTypeValue = (int)trackingType;
            int index = IndexOfInLocalTracking(trackingTypeValue);
            if (index != -1)
            {
                AttachToCurrentTrackingData(localTrackingTransforms[index], localPlayer, trackingType, toAttach);
                return;
            }

            Transform trackingTransform = GetAttachmentTransform();
            AttachToCurrentTrackingData(trackingTransform, localPlayer, trackingType, toAttach);

            index = localTrackingCount;
            IncrementLocalTrackingCount();
            localTrackingTypes[index] = trackingTypeValue;
            localTrackingTransforms[index] = trackingTransform;
        }

        public void AttachToBone(VRCPlayerApi player, HumanBodyBones bone, Transform toAttach)
        {
            int playerId = player.playerId;
            int boneValue = (int)bone;

            if (playerId == localPlayerId)
            {
                AttachToLocalPlayerBone(bone, boneValue, toAttach);
                return;
            }

            int index = IndexOfInNear(playerId, boneValue);
            if (index != -1)
            {
                AttachToBoneTransform(nearAttachedTransforms[index], player, bone, toAttach);
                return;
            }

            index = IndexOfInFar(playerId, boneValue);
            if (index != -1)
            {
                AttachToBoneTransform(farAttachedTransforms[index], player, bone, toAttach);
                return;
            }

            Transform boneTransform = GetAttachmentTransform();
            AttachToBoneTransform(boneTransform, player, bone, toAttach);

            if (IsNear(boneTransform.position))
            {
                index = nearAttachedCount;
                IncrementNearAttachedCount();
                nearAttachedPlayerIds[index] = playerId;
                nearAttachedPlayers[index] = player;
                nearAttachedBones[index] = boneValue;
                nearAttachedTransforms[index] = boneTransform;
            }
            else
            {
                index = farAttachedCount;
                IncrementFarAttachedCount();
                farAttachedPlayerIds[index] = playerId;
                farAttachedPlayers[index] = player;
                farAttachedBones[index] = boneValue;
                farAttachedTransforms[index] = boneTransform;
            }
        }

        private void AddToUnusedPrefabInstances(Transform unused)
        {
            // Reset just to dissuade from misusing the API - keeping a reference to this transform and using
            // it even after having detached from it. Which should not be done.
            unused.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            ArrList.Add(ref unusedPrefabInstances, ref unusedPrefabInstancesCount, unused);
        }

        private void DetachFromLocalPlayerBone(int boneValue)
        {
            int index = IndexOfInLocalBones(boneValue);
            if (index == -1)
                return;
            Transform boneTransform = localBoneTransforms[index];
            if (boneTransform.childCount != 0)
                return;
            AddToUnusedPrefabInstances(boneTransform);
            RemoveFromLocalBonesArrays(index);
        }

        private void DetachFromLocalTracking(int trackingTypeValue)
        {
            int index = IndexOfInLocalTracking(trackingTypeValue);
            if (index == -1)
                return;
            Transform trackingTransform = localTrackingTransforms[index];
            if (trackingTransform.childCount != 0)
                return;
            AddToUnusedPrefabInstances(trackingTransform);
            RemoveFromLocalTrackingArrays(index);
        }

        public void DetachFromBone(int playerId, HumanBodyBones bone, Transform toDetach)
        {
            int boneValue = (int)bone;

            if (toDetach != null)
                toDetach.SetParent(null); // Before the bone transform gets reset.

            if (playerId == localPlayerId)
            {
                DetachFromLocalPlayerBone(boneValue);
                return;
            }

            int index = IndexOfInNear(playerId, boneValue);
            if (index != -1)
            {
                Transform boneTransform = nearAttachedTransforms[index];
                if (boneTransform.childCount != 0)
                    return;
                AddToUnusedPrefabInstances(boneTransform);
                RemoveFromNearArrays(index);
                return;
            }

            index = IndexOfInFar(playerId, boneValue);
            if (index != -1)
            {
                Transform boneTransform = farAttachedTransforms[index];
                if (boneTransform.childCount != 0)
                    return;
                AddToUnusedPrefabInstances(boneTransform);
                RemoveFromFarArrays(index);
                return;
            }
        }

        public void DetachFromLocalTrackingData(VRCPlayerApi.TrackingDataType trackingType, Transform toDetach)
        {
            if (toDetach != null)
                toDetach.SetParent(null); // Before the tracking transform gets reset.
            DetachFromLocalTracking((int)trackingType);
        }
    }
}
