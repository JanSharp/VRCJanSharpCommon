﻿using UdonSharp;
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
        private bool[] nearAttachedIsPlayerTracking = new bool[ArrList.MinCapacity];
        private int[] nearAttachedBones = new int[ArrList.MinCapacity];
        private Transform[] nearAttachedTransforms = new Transform[ArrList.MinCapacity];
        private int[] nearAttachedCounts = new int[ArrList.MinCapacity];
        private int nearAttachedCount = 0;
        private int nearIncrementalIndex = 0;

        private int[] farAttachedPlayerIds = new int[ArrList.MinCapacity];
        private VRCPlayerApi[] farAttachedPlayers = new VRCPlayerApi[ArrList.MinCapacity];
        private bool[] farAttachedIsPlayerTracking = new bool[ArrList.MinCapacity];
        private int[] farAttachedBones = new int[ArrList.MinCapacity];
        private Transform[] farAttachedTransforms = new Transform[ArrList.MinCapacity];
        private int[] farAttachedCounts = new int[ArrList.MinCapacity];
        private int farAttachedCount = 0;
        private int farIncrementalIndex = 0;

        private int[] localBones = new int[ArrList.MinCapacity];
        private Transform[] localBoneTransforms = new Transform[ArrList.MinCapacity];
        private int[] localBoneCounts = new int[ArrList.MinCapacity];
        private int localBonesCount = 0;

        private int[] localTrackingTypes = new int[ArrList.MinCapacity];
        private Transform[] localTrackingTransforms = new Transform[ArrList.MinCapacity];
        private int[] localTrackingCounts = new int[ArrList.MinCapacity];
        private int localTrackingCount = 0;

        private const float NearDistanceThreshold = 32f;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            localPlayerId = localPlayer.playerId;
        }

        private void Update()
        {
            localPlayerPosition = localPlayer.GetPosition();
            DistanceCheckNearIncremental();
            UpdateNear();
            UpdateFarIncremental();
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
            for (int i = farAttachedCount - 1; i >= 0 ; i--)
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
            if (player == null || !player.IsValid())
                return;
            Vector3 position = nearAttachedIsPlayerTracking[nearIncrementalIndex]
                ? player.GetTrackingData((VRCPlayerApi.TrackingDataType)nearAttachedBones[nearIncrementalIndex]).position
                : player.GetBonePosition((HumanBodyBones)nearAttachedBones[nearIncrementalIndex]);
            // When zero, the bone does not exist and tracking it gets disabled. However it does not get
            // unregistered, so just keep it in the incremental far update loop.
            if (position == Vector3.zero || !IsNear(position))
                MoveFromNearToFar(nearIncrementalIndex);
            nearIncrementalIndex++;
        }

        private void UpdateNear()
        {
            for (int i = 0; i < nearAttachedCount; i++)
            {
                VRCPlayerApi player = nearAttachedPlayers[i];
                if (player == null || !player.IsValid())
                    continue;
                if (nearAttachedIsPlayerTracking[i])
                {
                    VRCPlayerApi.TrackingData data = player.GetTrackingData((VRCPlayerApi.TrackingDataType)nearAttachedBones[i]);
                    Vector3 position = data.position;
                    if (position != Vector3.zero)
                        nearAttachedTransforms[i].SetPositionAndRotation(position, data.rotation);
                }
                else
                {
                    HumanBodyBones bone = (HumanBodyBones)nearAttachedBones[i];
                    Vector3 position = player.GetBonePosition(bone);
                    if (position != Vector3.zero)
                        nearAttachedTransforms[i].SetPositionAndRotation(position, player.GetBoneRotation(bone));
                }
            }
        }

        private bool UpdateFarObject(int index)
        {
            VRCPlayerApi player = farAttachedPlayers[index];
            if (player == null || !player.IsValid())
                return false;
            Vector3 position;
            Quaternion rotation;
            if (farAttachedIsPlayerTracking[index])
            {
                VRCPlayerApi.TrackingData data = player.GetTrackingData((VRCPlayerApi.TrackingDataType)farAttachedBones[index]);
                position = data.position;
                rotation = data.rotation;
            }
            else
            {
                HumanBodyBones bone = (HumanBodyBones)farAttachedBones[index];
                position = player.GetBonePosition(bone);
                rotation = player.GetBoneRotation(bone);
            }
            if (position == Vector3.zero)
                return false;
            farAttachedTransforms[index].SetPositionAndRotation(position, rotation);
            bool isNear = IsNear(position);
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

        [OnTrulyPostLateUpdate]
        public void OnTrulyPostLateUpdate()
        {
            for (int i = 0; i < localBonesCount; i++)
            {
                HumanBodyBones bone = (HumanBodyBones)localBones[i];
                localBoneTransforms[i].SetPositionAndRotation(localPlayer.GetBonePosition(bone), localPlayer.GetBoneRotation(bone));
            }

            for (int i = 0; i < localTrackingCount; i++)
            {
                VRCPlayerApi.TrackingData data = localPlayer.GetTrackingData((VRCPlayerApi.TrackingDataType)localTrackingTypes[i]);
                localTrackingTransforms[i].SetPositionAndRotation(data.position, data.rotation);
            }
        }

        private bool IsNear(Vector3 bonePosition) => Vector3.Distance(localPlayerPosition, bonePosition) <= NearDistanceThreshold;

        private void MoveFromNearToFar(int nearIndex)
        {
            int farIndex = farAttachedCount;
            IncrementFarAttachedCount();
            farAttachedPlayerIds[farIndex] = nearAttachedPlayerIds[nearIndex];
            farAttachedPlayers[farIndex] = nearAttachedPlayers[nearIndex];
            farAttachedIsPlayerTracking[farIndex] = nearAttachedIsPlayerTracking[nearIndex];
            farAttachedBones[farIndex] = nearAttachedBones[nearIndex];
            farAttachedTransforms[farIndex] = nearAttachedTransforms[nearIndex];
            farAttachedCounts[farIndex] = nearAttachedCounts[nearIndex];
            RemoveFromNearArrays(nearIndex);
        }

        private void MoveFromFarToNear(int farIndex)
        {
            int nearIndex = nearAttachedCount;
            IncrementNearAttachedCount();
            nearAttachedPlayerIds[nearIndex] = farAttachedPlayerIds[farIndex];
            nearAttachedPlayers[nearIndex] = farAttachedPlayers[farIndex];
            nearAttachedIsPlayerTracking[nearIndex] = farAttachedIsPlayerTracking[farIndex];
            nearAttachedBones[nearIndex] = farAttachedBones[farIndex];
            nearAttachedTransforms[nearIndex] = farAttachedTransforms[farIndex];
            nearAttachedCounts[nearIndex] = farAttachedCounts[farIndex];
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
            nearAttachedIsPlayerTracking[nearIndex] = nearAttachedIsPlayerTracking[nearAttachedCount];
            nearAttachedBones[nearIndex] = nearAttachedBones[nearAttachedCount];
            nearAttachedTransforms[nearIndex] = nearAttachedTransforms[nearAttachedCount];
            nearAttachedCounts[nearIndex] = nearAttachedCounts[nearAttachedCount];
        }

        private void RemoveFromFarArrays(int farIndex)
        {
            // Move top of the arrays down to the removed index.
            farAttachedCount--;
            if (farIndex == farAttachedCount) // Just an optimization.
                return;
            farAttachedPlayerIds[farIndex] = farAttachedPlayerIds[farAttachedCount];
            farAttachedPlayers[farIndex] = farAttachedPlayers[farAttachedCount];
            farAttachedIsPlayerTracking[farIndex] = farAttachedIsPlayerTracking[farAttachedCount];
            farAttachedBones[farIndex] = farAttachedBones[farAttachedCount];
            farAttachedTransforms[farIndex] = farAttachedTransforms[farAttachedCount];
            farAttachedCounts[farIndex] = farAttachedCounts[farAttachedCount];
        }

        private void RemoveFromLocalBonesArrays(int index)
        {
            // Move top of the arrays down to the removed index.
            localBonesCount--;
            if (index == localBonesCount) // Just an optimization.
                return;
            localBones[index] = localBones[localBonesCount];
            localBoneTransforms[index] = localBoneTransforms[localBonesCount];
            localBoneCounts[index] = localBoneCounts[localBonesCount];
        }

        private void RemoveFromLocalTrackingArrays(int index)
        {
            // Move top of the arrays down to the removed index.
            localTrackingCount--;
            if (index == localTrackingCount) // Just an optimization.
                return;
            localTrackingTypes[index] = localTrackingTypes[localTrackingCount];
            localTrackingTransforms[index] = localTrackingTransforms[localTrackingCount];
            localTrackingCounts[index] = localTrackingCounts[localTrackingCount];
        }

        private void IncrementNearAttachedCount()
        {
            nearAttachedCount++;
            ArrList.EnsureCapacity(ref nearAttachedPlayerIds, nearAttachedCount);
            ArrList.EnsureCapacity(ref nearAttachedPlayers, nearAttachedCount);
            ArrList.EnsureCapacity(ref nearAttachedIsPlayerTracking, nearAttachedCount);
            ArrList.EnsureCapacity(ref nearAttachedBones, nearAttachedCount);
            ArrList.EnsureCapacity(ref nearAttachedTransforms, nearAttachedCount);
            ArrList.EnsureCapacity(ref nearAttachedCounts, nearAttachedCount);
        }

        private void IncrementFarAttachedCount()
        {
            farAttachedCount++;
            ArrList.EnsureCapacity(ref farAttachedPlayerIds, farAttachedCount);
            ArrList.EnsureCapacity(ref farAttachedPlayers, farAttachedCount);
            ArrList.EnsureCapacity(ref farAttachedIsPlayerTracking, farAttachedCount);
            ArrList.EnsureCapacity(ref farAttachedBones, farAttachedCount);
            ArrList.EnsureCapacity(ref farAttachedTransforms, farAttachedCount);
            ArrList.EnsureCapacity(ref farAttachedCounts, farAttachedCount);
        }

        private void IncrementLocalBonesCount()
        {
            localBonesCount++;
            ArrList.EnsureCapacity(ref localBones, localBonesCount);
            ArrList.EnsureCapacity(ref localBoneTransforms, localBonesCount);
            ArrList.EnsureCapacity(ref localBoneCounts, localBonesCount);
        }

        private void IncrementLocalTrackingCount()
        {
            localTrackingCount++;
            ArrList.EnsureCapacity(ref localTrackingTypes, localTrackingCount);
            ArrList.EnsureCapacity(ref localTrackingTransforms, localTrackingCount);
            ArrList.EnsureCapacity(ref localTrackingCounts, localTrackingCount);
        }

        private static int IndexOfInternal<T1, T2, T3>(T1[] arrayOne, T2[] arrayTwo, T3[] arrayThree, int listCount, T1 valueOne, T2 valueTwo, T3 valueThree)
        {
            int index = -1;
            while (true)
            {
                index = System.Array.IndexOf(arrayOne, valueOne, index + 1, listCount);
                if (index == -1 || (arrayTwo[index].Equals(valueTwo) && arrayThree[index].Equals(valueThree)))
                    return index;
            }
        }
        private int IndexOfInNear(int playerId, bool playerTracking, int bone)
            => IndexOfInternal(nearAttachedPlayerIds, nearAttachedIsPlayerTracking, nearAttachedBones, nearAttachedCount, playerId, playerTracking, bone);
        private int IndexOfInFar(int playerId, bool playerTracking, int bone)
            => IndexOfInternal(farAttachedPlayerIds, farAttachedIsPlayerTracking, farAttachedBones, farAttachedCount, playerId, playerTracking, bone);

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
                localBoneCounts[index]++;
                AttachToBoneTransform(localBoneTransforms[index], localPlayer, bone, toAttach);
                return;
            }

            Transform boneTransform = GetAttachmentTransform();
            AttachToBoneTransform(boneTransform, localPlayer, bone, toAttach);

            index = localBonesCount;
            IncrementLocalBonesCount();
            localBones[index] = boneValue;
            localBoneTransforms[index] = boneTransform;
            localBoneCounts[index] = 1;
        }

        public void AttachToLocalTrackingData(VRCPlayerApi.TrackingDataType trackingType, Transform toAttach)
        {
            int trackingTypeValue = (int)trackingType;
            int index = IndexOfInLocalTracking(trackingTypeValue);
            if (index != -1)
            {
                localTrackingCounts[index]++;
                AttachToCurrentTrackingData(localTrackingTransforms[index], localPlayer, trackingType, toAttach);
                return;
            }

            Transform trackingTransform = GetAttachmentTransform();
            AttachToCurrentTrackingData(trackingTransform, localPlayer, trackingType, toAttach);

            index = localTrackingCount;
            IncrementLocalTrackingCount();
            localTrackingTypes[index] = trackingTypeValue;
            localTrackingTransforms[index] = trackingTransform;
            localTrackingCounts[index] = 1;
        }

        public void AttachToBone(VRCPlayerApi player, HumanBodyBones bone, Transform toAttach)
        {
            AttachToRemotePlayer(player, false, (int)bone, toAttach);
        }

        public void AttachToTrackingData(VRCPlayerApi player, VRCPlayerApi.TrackingDataType trackingType, Transform toAttach)
        {
            AttachToRemotePlayer(player, true, (int)trackingType, toAttach);
        }

        private void AttachToRemotePlayer(VRCPlayerApi player, bool playerTracking, int boneValue, Transform toAttach)
        {
            int playerId = player.playerId;

            if (playerId == localPlayerId)
            {
                if (playerTracking)
                    AttachToLocalTrackingData((VRCPlayerApi.TrackingDataType)boneValue, toAttach);
                else
                    AttachToLocalPlayerBone((HumanBodyBones)boneValue, boneValue, toAttach);
                return;
            }

            int index = IndexOfInNear(playerId, true, boneValue);
            if (index != -1)
            {
                nearAttachedCounts[index]++;
                if (playerTracking)
                    AttachToCurrentTrackingData(nearAttachedTransforms[index], player, (VRCPlayerApi.TrackingDataType)boneValue, toAttach);
                else
                    AttachToBoneTransform(nearAttachedTransforms[index], player, (HumanBodyBones)boneValue, toAttach);
                return;
            }

            index = IndexOfInFar(playerId, true, boneValue);
            if (index != -1)
            {
                farAttachedCounts[index]++;
                if (playerTracking)
                    AttachToCurrentTrackingData(farAttachedTransforms[index], player, (VRCPlayerApi.TrackingDataType)boneValue, toAttach);
                else
                    AttachToBoneTransform(farAttachedTransforms[index], player, (HumanBodyBones)boneValue, toAttach);
                return;
            }

            Transform boneTransform = GetAttachmentTransform();
            if (playerTracking)
                AttachToCurrentTrackingData(boneTransform, player, (VRCPlayerApi.TrackingDataType)boneValue, toAttach);
            else
                AttachToBoneTransform(boneTransform, player, (HumanBodyBones)boneValue, toAttach);

            if (IsNear(boneTransform.position))
            {
                index = nearAttachedCount;
                IncrementNearAttachedCount();
                nearAttachedPlayerIds[index] = playerId;
                nearAttachedPlayers[index] = player;
                nearAttachedIsPlayerTracking[index] = playerTracking;
                nearAttachedBones[index] = boneValue;
                nearAttachedTransforms[index] = boneTransform;
                nearAttachedCounts[index] = 1;
            }
            else
            {
                index = farAttachedCount;
                IncrementFarAttachedCount();
                farAttachedPlayerIds[index] = playerId;
                farAttachedPlayers[index] = player;
                farAttachedIsPlayerTracking[index] = playerTracking;
                farAttachedBones[index] = boneValue;
                farAttachedTransforms[index] = boneTransform;
                farAttachedCounts[index] = 1;
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
            int count = --localBoneCounts[index];
            if (count != 0)
                return;
            AddToUnusedPrefabInstances(localBoneTransforms[index]);
            RemoveFromLocalBonesArrays(index);
        }

        private void DetachFromLocalTracking(int trackingTypeValue)
        {
            int index = IndexOfInLocalTracking(trackingTypeValue);
            if (index == -1)
                return;
            int count = --localTrackingCounts[index];
            if (count != 0)
                return;
            AddToUnusedPrefabInstances(localTrackingTransforms[index]);
            RemoveFromLocalTrackingArrays(index);
        }

        public void DetachFromBone(int playerId, HumanBodyBones bone, Transform toDetach)
        {
            DetachFromRemotePlayer(playerId, false, (int)bone, toDetach);
        }

        public void DetachFromTrackingData(int playerId, VRCPlayerApi.TrackingDataType trackingType, Transform toDetach)
        {
            DetachFromRemotePlayer(playerId, true, (int)trackingType, toDetach);
        }

        private void DetachFromRemotePlayer(int playerId, bool playerTracking, int boneValue, Transform toDetach)
        {
            toDetach.SetParent(null); // Before the bone transform gets reset.

            if (playerId == localPlayerId)
            {
                DetachFromLocalPlayerBone(boneValue);
                return;
            }

            int index = IndexOfInNear(playerId, playerTracking, boneValue);
            if (index != -1)
            {
                if ((--nearAttachedCounts[index]) != 0)
                    return;
                AddToUnusedPrefabInstances(nearAttachedTransforms[index]);
                RemoveFromNearArrays(index);
                return;
            }

            index = IndexOfInFar(playerId, playerTracking, boneValue);
            if (index == -1)
            {
                if ((--farAttachedCounts[index]) != 0)
                    return;
                AddToUnusedPrefabInstances(farAttachedTransforms[index]);
                RemoveFromFarArrays(index);
                return;
            }
        }

        public void DetachFromLocalTrackingData(VRCPlayerApi.TrackingDataType trackingType, Transform toDetach)
        {
            toDetach.SetParent(null); // Before the tracking transform gets reset.
            DetachFromLocalTracking((int)trackingType);
        }

        // TODO: tell the user that this depends on the truly post late update prefab, and enforce that using editor scripting
    }
}
