using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
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

        private const int ToInterpolateIndex = 0;
        private const int StartTimeIndex = 1;
        private const int LerpDurationIndex = 2;
        private const int SourceValueIndex = 3;
        private const int DestinationValueIndex = 4;
        private const int CallbackUdonBehaviourIndex = 5;
        private const int CallbackEventNameIndex = 6;
        private const int CustomCallbackDataIndex = 7;
        private const int DefinitionSize = 8;

        private object[][] localPositionDefs = new object[ArrList.MinCapacity][];
        private int localPositionDefsCount = 0;
        private DataDictionary localPositionDefsLut = new DataDictionary();

        private object[][] localRotationDefs = new object[ArrList.MinCapacity][];
        private int localRotationDefsCount = 0;
        private DataDictionary localRotationDefsLut = new DataDictionary();

        private object[][] localScaleDefs = new object[ArrList.MinCapacity][];
        private int localScaleDefsCount = 0;
        private DataDictionary localScaleDefsLut = new DataDictionary();

        private Transform finishedTransform;
        public Transform FinishedTransform => finishedTransform;
        private object customCallbackData;
        public object CustomCallbackData => customCallbackData;

        private float time;

        private void UpdateLocalPositionDefs()
        {
            for (int i = localPositionDefsCount - 1; i >= 0; i--)
            {
                object[] def = localPositionDefs[i];
                Transform toInterpolate = (Transform)def[ToInterpolateIndex];
                if (toInterpolate == null)
                {
                    localPositionDefs[i] = localPositionDefs[--localPositionDefsCount];
                    localPositionDefsLut.Remove(toInterpolate); // TODO: does this work?
                    CallCallback(def, toInterpolate);
                    continue;
                }
                Vector3 destination = (Vector3)def[DestinationValueIndex];

                float startTime = (float)def[StartTimeIndex];
                float lerpDuration = (float)def[LerpDurationIndex];
                float t = (time - startTime) / lerpDuration;
                if (t < 1f)
                {
                    Vector3 source = (Vector3)def[SourceValueIndex];
                    toInterpolate.localPosition = Vector3.Lerp(source, destination, t);
                    continue;
                }
                toInterpolate.localPosition = destination;
                localPositionDefs[i] = localPositionDefs[--localPositionDefsCount];
                localPositionDefsLut.Remove(toInterpolate);
                CallCallback(def, toInterpolate);
            }
        }

        private void UpdateLocalRotationDefs()
        {
            for (int i = localRotationDefsCount - 1; i >= 0; i--)
            {
                object[] def = localRotationDefs[i];
                Transform toInterpolate = (Transform)def[ToInterpolateIndex];
                if (toInterpolate == null)
                {
                    localRotationDefs[i] = localRotationDefs[--localRotationDefsCount];
                    localRotationDefsLut.Remove(toInterpolate); // TODO: does this work?
                    CallCallback(def, toInterpolate);
                    continue;
                }
                Quaternion destination = (Quaternion)def[DestinationValueIndex];

                float startTime = (float)def[StartTimeIndex];
                float lerpDuration = (float)def[LerpDurationIndex];
                float t = (time - startTime) / lerpDuration;
                if (t < 1f)
                {
                    Quaternion source = (Quaternion)def[SourceValueIndex];
                    toInterpolate.localRotation = Quaternion.Lerp(source, destination, t);
                    continue;
                }
                toInterpolate.localRotation = destination;
                localRotationDefs[i] = localRotationDefs[--localRotationDefsCount];
                localRotationDefsLut.Remove(toInterpolate);
                CallCallback(def, toInterpolate);
            }
        }

        private void UpdateLocalScaleDefs()
        {
            for (int i = localScaleDefsCount - 1; i >= 0; i--)
            {
                object[] def = localScaleDefs[i];
                Transform toInterpolate = (Transform)def[ToInterpolateIndex];
                if (toInterpolate == null)
                {
                    localScaleDefs[i] = localScaleDefs[--localScaleDefsCount];
                    localScaleDefsLut.Remove(toInterpolate); // TODO: does this work?
                    CallCallback(def, toInterpolate);
                    continue;
                }
                Vector3 destination = (Vector3)def[DestinationValueIndex];

                float startTime = (float)def[StartTimeIndex];
                float lerpDuration = (float)def[LerpDurationIndex];
                float t = (time - startTime) / lerpDuration;
                if (t < 1f)
                {
                    Vector3 source = (Vector3)def[SourceValueIndex];
                    toInterpolate.localScale = Vector3.Lerp(source, destination, t);
                    continue;
                }
                toInterpolate.localScale = destination;
                localScaleDefs[i] = localScaleDefs[--localScaleDefsCount];
                localScaleDefsLut.Remove(toInterpolate);
                CallCallback(def, toInterpolate);
            }
        }

        private void CallCallback(object[] positionDef, Transform toInterpolate)
        {
            UdonSharpBehaviour callbackInst = (UdonSharpBehaviour)positionDef[CallbackUdonBehaviourIndex];
            if (callbackInst != null)
            {
                string callbackEventName = (string)positionDef[CallbackEventNameIndex];
                customCallbackData = positionDef[CustomCallbackDataIndex];
                finishedTransform = toInterpolate;
                callbackInst.SendCustomEvent(callbackEventName);
                finishedTransform = null;
                customCallbackData = null;
            }
        }

        public void CustomUpdate()
        {
            time = Time.time;
            bool doDeregister = true;
            if (localPositionDefsCount != 0)
            {
                UpdateLocalPositionDefs();
                doDeregister = false;
            }
            if (localRotationDefsCount != 0)
            {
                UpdateLocalRotationDefs();
                doDeregister = false;
            }
            if (localScaleDefsCount != 0)
            {
                UpdateLocalScaleDefs();
                doDeregister = false;
            }
            if (doDeregister)
                updateManager.Deregister(this);
        }

        public object[] LerpLocalPosition(
            Transform toInterpolate,
            Vector3 destinationLocalPosition,
            float lerpDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
            object[] def;
            DataToken keyToken = toInterpolate;
            if (localPositionDefsLut.TryGetValue(keyToken, out DataToken defToken))
                def = (object[])defToken.Reference;
            else
            {
                def = new object[DefinitionSize];
                localPositionDefsLut.Add(keyToken, new DataToken(def));
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[LerpDurationIndex] = lerpDuration;
            def[SourceValueIndex] = toInterpolate.localPosition;
            def[DestinationValueIndex] = destinationLocalPosition;
            def[CallbackUdonBehaviourIndex] = callbackInst;
            def[CallbackEventNameIndex] = callbackEventName;
            def[CustomCallbackDataIndex] = customCallbackData;
            ArrList.Add(ref localPositionDefs, ref localPositionDefsCount, def);
            updateManager.Register(this);
            return def;
        }

        public object[] LerpLocalRotation(
            Transform toInterpolate,
            Quaternion destinationLocalRotation,
            float lerpDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
            object[] def;
            DataToken keyToken = toInterpolate;
            if (localRotationDefsLut.TryGetValue(keyToken, out DataToken defToken))
                def = (object[])defToken.Reference;
            else
            {
                def = new object[DefinitionSize];
                localRotationDefsLut.Add(keyToken, new DataToken(def));
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[LerpDurationIndex] = lerpDuration;
            def[SourceValueIndex] = toInterpolate.localRotation;
            def[DestinationValueIndex] = destinationLocalRotation;
            def[CallbackUdonBehaviourIndex] = callbackInst;
            def[CallbackEventNameIndex] = callbackEventName;
            def[CustomCallbackDataIndex] = customCallbackData;
            ArrList.Add(ref localRotationDefs, ref localRotationDefsCount, def);
            updateManager.Register(this);
            return def;
        }

        public object[] LerpLocalScale(
            Transform toInterpolate,
            Vector3 destinationLocalScale,
            float lerpDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
            object[] def;
            DataToken keyToken = toInterpolate;
            if (localScaleDefsLut.TryGetValue(keyToken, out DataToken defToken))
                def = (object[])defToken.Reference;
            else
            {
                def = new object[DefinitionSize];
                localScaleDefsLut.Add(keyToken, new DataToken(def));
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[LerpDurationIndex] = lerpDuration;
            def[SourceValueIndex] = toInterpolate.localScale;
            def[DestinationValueIndex] = destinationLocalScale;
            def[CallbackUdonBehaviourIndex] = callbackInst;
            def[CallbackEventNameIndex] = callbackEventName;
            def[CustomCallbackDataIndex] = customCallbackData;
            ArrList.Add(ref localScaleDefs, ref localScaleDefsCount, def);
            updateManager.Register(this);
            return def;
        }

        public bool CancelLocalPositionLerp(Transform toInterpolate)
        {
            if (!localPositionDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null;
            return true;
        }

        public bool CancelLocalRotationLerp(Transform toInterpolate)
        {
            if (!localRotationDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null;
            return true;
        }

        public bool CancelLocalScaleLerp(Transform toInterpolate)
        {
            if (!localScaleDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null;
            return true;
        }
    }
}
