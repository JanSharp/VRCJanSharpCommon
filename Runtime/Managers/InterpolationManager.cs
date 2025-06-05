using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonScript("ba15cd230a6c132138cc3a7d2d0e91e5")] // Runtime/Prefabs/InterpolationManager.prefab
    public class InterpolationManager : UdonSharpBehaviour
    {
        [HideInInspector][SerializeField][SingletonReference] private UpdateManager updateManager;
        [System.NonSerialized] public int customUpdateInternalIndex; // Required for the UpdateManager.

        private const int LocalPositionFlag = 1;
        private const int LocalRotationFlag = 2;
        private const int LocalScaleFlag = 4;

        private const int LerpTypeIndex = 0;
        private const int ToLerpIndex = 1;
        private const int StartTimeIndex = 2;
        private const int LerpDurationIndex = 3;
        private const int SourceLocalPositionIndex = 4;
        private const int DestinationLocalPositionIndex = 5;
        private const int SourceLocalRotationIndex = 6;
        private const int DestinationLocalRotationIndex = 7;
        private const int SourceLocalScaleIndex = 8;
        private const int DestinationLocalScaleIndex = 9;
        private const int CallbackUdonBehaviourIndex = 10;
        private const int CallbackEventNameIndex = 11;
        private const int CustomCallbackDataIndex = 12;
        private const int DefinitionSize = 13;

        private object[][] activeLerpDefs = new object[ArrList.MinCapacity][];
        private int activeLerpDefsCount = 0;

        private Transform finishedTransform;
        public Transform FinishedTransform => finishedTransform;
        private object customCallbackData;
        public object CustomCallbackData => customCallbackData;

        public void CustomUpdate()
        {
            float time = Time.time;
            for (int i = activeLerpDefsCount - 1; i >= 0; i--)
            {
                object[] lerpDef = activeLerpDefs[i];
                int lerpType = (int)lerpDef[LerpTypeIndex];
                Transform toLerp = (Transform)lerpDef[ToLerpIndex];
                if (toLerp == null)
                {
                    RemoveLerpDef(i, lerpDef, toLerp);
                    continue;
                }

                float startTime = (float)lerpDef[StartTimeIndex];
                float lerpDuration = (float)lerpDef[LerpDurationIndex];
                float t = (time - startTime) / lerpDuration;
                if (t < 1f)
                {
                    // We are building mountains. Ew. But inlining everything is best for performance with Udon...
                    if ((lerpType & LocalPositionFlag) != 0)
                    {
                        Vector3 sourceLocalPosition = (Vector3)lerpDef[SourceLocalPositionIndex];
                        Vector3 destinationLocalPosition = (Vector3)lerpDef[DestinationLocalPositionIndex];
                        toLerp.localPosition = Vector3.Lerp(sourceLocalPosition, destinationLocalPosition, t);
                    }
                    if ((lerpType & LocalRotationFlag) != 0)
                    {
                        Quaternion sourceLocalRotation = (Quaternion)lerpDef[SourceLocalRotationIndex];
                        Quaternion destinationLocalRotation = (Quaternion)lerpDef[DestinationLocalRotationIndex];
                        toLerp.localRotation = Quaternion.Lerp(sourceLocalRotation, destinationLocalRotation, t);
                    }
                    if ((lerpType & LocalScaleFlag) != 0)
                    {
                        Vector3 sourceLocalScale = (Vector3)lerpDef[SourceLocalScaleIndex];
                        Vector3 destinationLocalScale = (Vector3)lerpDef[DestinationLocalScaleIndex];
                        toLerp.localScale = Vector3.Lerp(sourceLocalScale, destinationLocalScale, t);
                    }
                    continue;
                }

                if ((lerpType & LocalPositionFlag) != 0)
                {
                    Vector3 destinationLocalPosition = (Vector3)lerpDef[DestinationLocalPositionIndex];
                    toLerp.localPosition = destinationLocalPosition;
                }
                if ((lerpType & LocalRotationFlag) != 0)
                {
                    Quaternion destinationLocalRotation = (Quaternion)lerpDef[DestinationLocalRotationIndex];
                    toLerp.localRotation = destinationLocalRotation;
                }
                if ((lerpType & LocalScaleFlag) != 0)
                {
                    Vector3 destinationLocalScale = (Vector3)lerpDef[DestinationLocalScaleIndex];
                    toLerp.localScale = destinationLocalScale;
                }

                RemoveLerpDef(i, lerpDef, toLerp);
            }

            if (activeLerpDefsCount == 0)
                updateManager.Deregister(this);
        }

        private void RemoveLerpDef(int index, object[] lerpDef, Transform toLerp)
        {
            activeLerpDefs[index] = activeLerpDefs[--activeLerpDefsCount];
            UdonSharpBehaviour callbackInst = (UdonSharpBehaviour)lerpDef[CallbackUdonBehaviourIndex];
            if (callbackInst != null)
            {
                string callbackEventName = (string)lerpDef[CallbackEventNameIndex];
                customCallbackData = lerpDef[CustomCallbackDataIndex];
                finishedTransform = toLerp;
                callbackInst.SendCustomEvent(callbackEventName);
                finishedTransform = null;
                customCallbackData = null;
            }
        }

        public object[] LerpLocalPosition(
            Transform toLerp,
            Vector3 destinationLocalPosition,
            float lerpDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
            object[] lerpDefinition = new object[DefinitionSize];
            lerpDefinition[LerpTypeIndex] = LocalPositionFlag;
            lerpDefinition[ToLerpIndex] = toLerp;
            lerpDefinition[StartTimeIndex] = Time.time;
            lerpDefinition[LerpDurationIndex] = lerpDuration;
            lerpDefinition[SourceLocalPositionIndex] = toLerp.localPosition;
            lerpDefinition[DestinationLocalPositionIndex] = destinationLocalPosition;
            // lerpDefinition[SourceLocalRotationIndex] = default;
            // lerpDefinition[DestinationLocalRotationIndex] = default;
            // lerpDefinition[SourceLocalScaleIndex] = default;
            // lerpDefinition[DestinationLocalScaleIndex] = default;
            lerpDefinition[CallbackUdonBehaviourIndex] = callbackInst;
            lerpDefinition[CallbackEventNameIndex] = callbackEventName;
            lerpDefinition[CustomCallbackDataIndex] = customCallbackData;
            ArrList.Add(ref activeLerpDefs, ref activeLerpDefsCount, lerpDefinition);
            if (activeLerpDefsCount == 1)
                updateManager.Register(this);
            return lerpDefinition;
        }

        public object[] LerpLocalRotation(
            Transform toLerp,
            Quaternion destinationLocalRotation,
            float lerpDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
            object[] lerpDefinition = new object[DefinitionSize];
            lerpDefinition[LerpTypeIndex] = LocalRotationFlag;
            lerpDefinition[ToLerpIndex] = toLerp;
            lerpDefinition[StartTimeIndex] = Time.time;
            lerpDefinition[LerpDurationIndex] = lerpDuration;
            // lerpDefinition[SourceLocalPositionIndex] = default;
            // lerpDefinition[DestinationLocalPositionIndex] = default;
            lerpDefinition[SourceLocalRotationIndex] = toLerp.localRotation;
            lerpDefinition[DestinationLocalRotationIndex] = destinationLocalRotation;
            // lerpDefinition[SourceLocalScaleIndex] = default;
            // lerpDefinition[DestinationLocalScaleIndex] = default;
            lerpDefinition[CallbackUdonBehaviourIndex] = callbackInst;
            lerpDefinition[CallbackEventNameIndex] = callbackEventName;
            lerpDefinition[CustomCallbackDataIndex] = customCallbackData;
            ArrList.Add(ref activeLerpDefs, ref activeLerpDefsCount, lerpDefinition);
            if (activeLerpDefsCount == 1)
                updateManager.Register(this);
            return lerpDefinition;
        }

        public object[] LerpLocalScale(
            Transform toLerp,
            Vector3 destinationLocalScale,
            float lerpDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
            object[] lerpDefinition = new object[DefinitionSize];
            lerpDefinition[LerpTypeIndex] = LocalScaleFlag;
            lerpDefinition[ToLerpIndex] = toLerp;
            lerpDefinition[StartTimeIndex] = Time.time;
            lerpDefinition[LerpDurationIndex] = lerpDuration;
            // lerpDefinition[SourceLocalPositionIndex] = default;
            // lerpDefinition[DestinationLocalPositionIndex] = default;
            // lerpDefinition[SourceLocalRotationIndex] = default;
            // lerpDefinition[DestinationLocalRotationIndex] = default;
            lerpDefinition[SourceLocalScaleIndex] = toLerp.localScale;
            lerpDefinition[DestinationLocalScaleIndex] = destinationLocalScale;
            lerpDefinition[CallbackUdonBehaviourIndex] = callbackInst;
            lerpDefinition[CallbackEventNameIndex] = callbackEventName;
            lerpDefinition[CustomCallbackDataIndex] = customCallbackData;
            ArrList.Add(ref activeLerpDefs, ref activeLerpDefsCount, lerpDefinition);
            if (activeLerpDefsCount == 1)
                updateManager.Register(this);
            return lerpDefinition;
        }

        public object[] LerpLocalPositionAndRotation(
            Transform toLerp,
            Vector3 destinationLocalPosition,
            Quaternion destinationLocalRotation,
            float lerpDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
            object[] lerpDefinition = new object[DefinitionSize];
            lerpDefinition[LerpTypeIndex] = LocalPositionFlag | LocalRotationFlag;
            lerpDefinition[ToLerpIndex] = toLerp;
            lerpDefinition[StartTimeIndex] = Time.time;
            lerpDefinition[LerpDurationIndex] = lerpDuration;
            lerpDefinition[SourceLocalPositionIndex] = toLerp.localPosition;
            lerpDefinition[DestinationLocalPositionIndex] = destinationLocalPosition;
            lerpDefinition[SourceLocalRotationIndex] = toLerp.localRotation;
            lerpDefinition[DestinationLocalRotationIndex] = destinationLocalRotation;
            // lerpDefinition[SourceLocalScaleIndex] = default;
            // lerpDefinition[DestinationLocalScaleIndex] = default;
            lerpDefinition[CallbackUdonBehaviourIndex] = callbackInst;
            lerpDefinition[CallbackEventNameIndex] = callbackEventName;
            lerpDefinition[CustomCallbackDataIndex] = customCallbackData;
            ArrList.Add(ref activeLerpDefs, ref activeLerpDefsCount, lerpDefinition);
            if (activeLerpDefsCount == 1)
                updateManager.Register(this);
            return lerpDefinition;
        }

        public object[] LerpLocalPositionAndScale(
            Transform toLerp,
            Vector3 destinationLocalPosition,
            Vector3 destinationLocalScale,
            float lerpDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
            object[] lerpDefinition = new object[DefinitionSize];
            lerpDefinition[LerpTypeIndex] = LocalPositionFlag | LocalScaleFlag;
            lerpDefinition[ToLerpIndex] = toLerp;
            lerpDefinition[StartTimeIndex] = Time.time;
            lerpDefinition[LerpDurationIndex] = lerpDuration;
            lerpDefinition[SourceLocalPositionIndex] = toLerp.localPosition;
            lerpDefinition[DestinationLocalPositionIndex] = destinationLocalPosition;
            // lerpDefinition[SourceLocalRotationIndex] = default;
            // lerpDefinition[DestinationLocalRotationIndex] = default;
            lerpDefinition[SourceLocalScaleIndex] = toLerp.localScale;
            lerpDefinition[DestinationLocalScaleIndex] = destinationLocalScale;
            lerpDefinition[CallbackUdonBehaviourIndex] = callbackInst;
            lerpDefinition[CallbackEventNameIndex] = callbackEventName;
            lerpDefinition[CustomCallbackDataIndex] = customCallbackData;
            ArrList.Add(ref activeLerpDefs, ref activeLerpDefsCount, lerpDefinition);
            if (activeLerpDefsCount == 1)
                updateManager.Register(this);
            return lerpDefinition;
        }

        public object[] LerpLocalRotationAndScale(
            Transform toLerp,
            Quaternion destinationLocalRotation,
            Vector3 destinationLocalScale,
            float lerpDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
            object[] lerpDefinition = new object[DefinitionSize];
            lerpDefinition[LerpTypeIndex] = LocalRotationFlag | LocalScaleFlag;
            lerpDefinition[ToLerpIndex] = toLerp;
            lerpDefinition[StartTimeIndex] = Time.time;
            lerpDefinition[LerpDurationIndex] = lerpDuration;
            // lerpDefinition[SourceLocalPositionIndex] = default;
            // lerpDefinition[DestinationLocalPositionIndex] = default;
            lerpDefinition[SourceLocalRotationIndex] = toLerp.localRotation;
            lerpDefinition[DestinationLocalRotationIndex] = destinationLocalRotation;
            lerpDefinition[SourceLocalScaleIndex] = toLerp.localScale;
            lerpDefinition[DestinationLocalScaleIndex] = destinationLocalScale;
            lerpDefinition[CallbackUdonBehaviourIndex] = callbackInst;
            lerpDefinition[CallbackEventNameIndex] = callbackEventName;
            lerpDefinition[CustomCallbackDataIndex] = customCallbackData;
            ArrList.Add(ref activeLerpDefs, ref activeLerpDefsCount, lerpDefinition);
            if (activeLerpDefsCount == 1)
                updateManager.Register(this);
            return lerpDefinition;
        }

        public object[] LerpLocalPositionAndRotationAndScale(
            Transform toLerp,
            Vector3 destinationLocalPosition,
            Quaternion destinationLocalRotation,
            Vector3 destinationLocalScale,
            float lerpDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
            object[] lerpDefinition = new object[DefinitionSize];
            lerpDefinition[LerpTypeIndex] = LocalPositionFlag | LocalRotationFlag | LocalScaleFlag;
            lerpDefinition[ToLerpIndex] = toLerp;
            lerpDefinition[StartTimeIndex] = Time.time;
            lerpDefinition[LerpDurationIndex] = lerpDuration;
            lerpDefinition[SourceLocalPositionIndex] = toLerp.localPosition;
            lerpDefinition[DestinationLocalPositionIndex] = destinationLocalPosition;
            lerpDefinition[SourceLocalRotationIndex] = toLerp.localRotation;
            lerpDefinition[DestinationLocalRotationIndex] = destinationLocalRotation;
            lerpDefinition[SourceLocalScaleIndex] = toLerp.localScale;
            lerpDefinition[DestinationLocalScaleIndex] = destinationLocalScale;
            lerpDefinition[CallbackUdonBehaviourIndex] = callbackInst;
            lerpDefinition[CallbackEventNameIndex] = callbackEventName;
            lerpDefinition[CustomCallbackDataIndex] = customCallbackData;
            ArrList.Add(ref activeLerpDefs, ref activeLerpDefsCount, lerpDefinition);
            if (activeLerpDefsCount == 1)
                updateManager.Register(this);
            return lerpDefinition;
        }
    }
}
