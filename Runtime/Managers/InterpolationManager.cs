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
        private const int InterpolationDurationIndex = 2;
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

        private object[][] worldPositionDefs = new object[ArrList.MinCapacity][];
        private int worldPositionDefsCount = 0;
        private DataDictionary worldPositionDefsLut = new DataDictionary();

        private object[][] worldRotationDefs = new object[ArrList.MinCapacity][];
        private int worldRotationDefsCount = 0;
        private DataDictionary worldRotationDefsLut = new DataDictionary();

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
                    // Cannot remove from the lut using 'null' as the key - that does nothing - so cannot clean up.
                    CallCallback(def, toInterpolate);
                    continue;
                }
                Vector3 destination = (Vector3)def[DestinationValueIndex];

                float startTime = (float)def[StartTimeIndex];
                float interpolationDuration = (float)def[InterpolationDurationIndex];
                float t = (time - startTime) / interpolationDuration;
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
                    // Cannot remove from the lut using 'null' as the key - that does nothing - so cannot clean up.
                    CallCallback(def, toInterpolate);
                    continue;
                }
                Quaternion destination = (Quaternion)def[DestinationValueIndex];

                float startTime = (float)def[StartTimeIndex];
                float interpolationDuration = (float)def[InterpolationDurationIndex];
                float t = (time - startTime) / interpolationDuration;
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
                    // Cannot remove from the lut using 'null' as the key - that does nothing - so cannot clean up.
                    CallCallback(def, toInterpolate);
                    continue;
                }
                Vector3 destination = (Vector3)def[DestinationValueIndex];

                float startTime = (float)def[StartTimeIndex];
                float interpolationDuration = (float)def[InterpolationDurationIndex];
                float t = (time - startTime) / interpolationDuration;
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

        private void UpdateWorldPositionDefs()
        {
            for (int i = worldPositionDefsCount - 1; i >= 0; i--)
            {
                object[] def = worldPositionDefs[i];
                Transform toInterpolate = (Transform)def[ToInterpolateIndex];
                if (toInterpolate == null)
                {
                    worldPositionDefs[i] = worldPositionDefs[--worldPositionDefsCount];
                    // Cannot remove from the lut using 'null' as the key - that does nothing - so cannot clean up.
                    CallCallback(def, toInterpolate);
                    continue;
                }
                Vector3 destination = (Vector3)def[DestinationValueIndex];

                float startTime = (float)def[StartTimeIndex];
                float interpolationDuration = (float)def[InterpolationDurationIndex];
                float t = (time - startTime) / interpolationDuration;
                if (t < 1f)
                {
                    Vector3 source = (Vector3)def[SourceValueIndex];
                    toInterpolate.position = Vector3.Lerp(source, destination, t);
                    continue;
                }
                toInterpolate.position = destination;
                worldPositionDefs[i] = worldPositionDefs[--worldPositionDefsCount];
                worldPositionDefsLut.Remove(toInterpolate);
                CallCallback(def, toInterpolate);
            }
        }

        private void UpdateWorldRotationDefs()
        {
            for (int i = worldRotationDefsCount - 1; i >= 0; i--)
            {
                object[] def = worldRotationDefs[i];
                Transform toInterpolate = (Transform)def[ToInterpolateIndex];
                if (toInterpolate == null)
                {
                    worldRotationDefs[i] = worldRotationDefs[--worldRotationDefsCount];
                    // Cannot remove from the lut using 'null' as the key - that does nothing - so cannot clean up.
                    CallCallback(def, toInterpolate);
                    continue;
                }
                Quaternion destination = (Quaternion)def[DestinationValueIndex];

                float startTime = (float)def[StartTimeIndex];
                float interpolationDuration = (float)def[InterpolationDurationIndex];
                float t = (time - startTime) / interpolationDuration;
                if (t < 1f)
                {
                    Quaternion source = (Quaternion)def[SourceValueIndex];
                    toInterpolate.rotation = Quaternion.Lerp(source, destination, t);
                    continue;
                }
                toInterpolate.rotation = destination;
                worldRotationDefs[i] = worldRotationDefs[--worldRotationDefsCount];
                worldRotationDefsLut.Remove(toInterpolate);
                CallCallback(def, toInterpolate);
            }
        }

        private void CallCallback(object[] positionDef, Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CallCallback");
#endif
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
            if (worldPositionDefsCount != 0)
            {
                UpdateWorldPositionDefs();
                doDeregister = false;
            }
            if (worldRotationDefsCount != 0)
            {
                UpdateWorldRotationDefs();
                doDeregister = false;
            }

            if (doDeregister)
            {
#if JAN_SHARP_COMMON_DEBUG
                Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CustomUpdate (inner) - localPositionDefsCount: {localPositionDefsCount}, localRotationDefsCount: {localRotationDefsCount}, localScaleDefsCount: {localScaleDefsCount}, worldPositionDefsCount: {worldPositionDefsCount}, worldRotationDefsCount: {worldRotationDefsCount}");
#endif
                updateManager.Deregister(this);
            }
        }

        public object[] InterpolateLocalPosition(
            Transform toInterpolate,
            Vector3 destinationLocalPosition,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateLocalPosition");
#endif
            CancelWorldPositionInterpolation(toInterpolate);
            object[] def;
            DataToken keyToken = toInterpolate;
            if (localPositionDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                def = (object[])defToken.Reference;
                CallCallback(def, toInterpolate);
            }
            else
            {
                def = new object[DefinitionSize];
                localPositionDefsLut.Add(keyToken, new DataToken(def));
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[InterpolationDurationIndex] = interpolationDuration;
            def[SourceValueIndex] = toInterpolate.localPosition;
            def[DestinationValueIndex] = destinationLocalPosition;
            ArrList.Add(ref localPositionDefs, ref localPositionDefsCount, def);
            updateManager.Register(this);
            return def;
        }

        public object[] InterpolateLocalPosition(
            Transform toInterpolate,
            Vector3 destinationLocalPosition,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateLocalPosition");
#endif
            object[] def = InterpolateLocalPosition(toInterpolate, destinationLocalPosition, interpolationDuration);
            def[CallbackUdonBehaviourIndex] = callbackInst;
            def[CallbackEventNameIndex] = callbackEventName;
            def[CustomCallbackDataIndex] = customCallbackData;
            return def;
        }

        public object[] InterpolateLocalRotation(
            Transform toInterpolate,
            Quaternion destinationLocalRotation,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateLocalRotation");
#endif
            CancelWorldRotationInterpolation(toInterpolate);
            object[] def;
            DataToken keyToken = toInterpolate;
            if (localRotationDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                def = (object[])defToken.Reference;
                CallCallback(def, toInterpolate);
            }
            else
            {
                def = new object[DefinitionSize];
                localRotationDefsLut.Add(keyToken, new DataToken(def));
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[InterpolationDurationIndex] = interpolationDuration;
            def[SourceValueIndex] = toInterpolate.localRotation;
            def[DestinationValueIndex] = destinationLocalRotation;
            ArrList.Add(ref localRotationDefs, ref localRotationDefsCount, def);
            updateManager.Register(this);
            return def;
        }

        public object[] InterpolateLocalRotation(
            Transform toInterpolate,
            Quaternion destinationLocalRotation,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateLocalRotation");
#endif
            object[] def = InterpolateLocalRotation(toInterpolate, destinationLocalRotation, interpolationDuration);
            def[CallbackUdonBehaviourIndex] = callbackInst;
            def[CallbackEventNameIndex] = callbackEventName;
            def[CustomCallbackDataIndex] = customCallbackData;
            return def;
        }

        public object[] InterpolateLocalScale(
            Transform toInterpolate,
            Vector3 destinationLocalScale,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateLocalScale");
#endif
            object[] def;
            DataToken keyToken = toInterpolate;
            if (localScaleDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                def = (object[])defToken.Reference;
                CallCallback(def, toInterpolate);
            }
            else
            {
                def = new object[DefinitionSize];
                localScaleDefsLut.Add(keyToken, new DataToken(def));
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[InterpolationDurationIndex] = interpolationDuration;
            def[SourceValueIndex] = toInterpolate.localScale;
            def[DestinationValueIndex] = destinationLocalScale;
            ArrList.Add(ref localScaleDefs, ref localScaleDefsCount, def);
            updateManager.Register(this);
            return def;
        }

        public object[] InterpolateLocalScale(
            Transform toInterpolate,
            Vector3 destinationLocalScale,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateLocalScale");
#endif
            object[] def = InterpolateLocalScale(toInterpolate, destinationLocalScale, interpolationDuration);
            def[CallbackUdonBehaviourIndex] = callbackInst;
            def[CallbackEventNameIndex] = callbackEventName;
            def[CustomCallbackDataIndex] = customCallbackData;
            return def;
        }

        public object[] InterpolateWorldPosition(
            Transform toInterpolate,
            Vector3 destinationWorldPosition,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateWorldPosition");
#endif
            CancelLocalPositionInterpolation(toInterpolate);
            object[] def;
            DataToken keyToken = toInterpolate;
            if (worldPositionDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                def = (object[])defToken.Reference;
                CallCallback(def, toInterpolate);
            }
            else
            {
                def = new object[DefinitionSize];
                worldPositionDefsLut.Add(keyToken, new DataToken(def));
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[InterpolationDurationIndex] = interpolationDuration;
            def[SourceValueIndex] = toInterpolate.position;
            def[DestinationValueIndex] = destinationWorldPosition;
            ArrList.Add(ref worldPositionDefs, ref worldPositionDefsCount, def);
            updateManager.Register(this);
            return def;
        }

        public object[] InterpolateWorldPosition(
            Transform toInterpolate,
            Vector3 destinationWorldPosition,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateWorldPosition");
#endif
            object[] def = InterpolateWorldPosition(toInterpolate, destinationWorldPosition, interpolationDuration);
            def[CallbackUdonBehaviourIndex] = callbackInst;
            def[CallbackEventNameIndex] = callbackEventName;
            def[CustomCallbackDataIndex] = customCallbackData;
            return def;
        }

        public object[] InterpolateWorldRotation(
            Transform toInterpolate,
            Quaternion destinationWorldRotation,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateWorldRotation");
#endif
            CancelLocalRotationInterpolation(toInterpolate);
            object[] def;
            DataToken keyToken = toInterpolate;
            if (worldRotationDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                def = (object[])defToken.Reference;
                CallCallback(def, toInterpolate);
            }
            else
            {
                def = new object[DefinitionSize];
                worldRotationDefsLut.Add(keyToken, new DataToken(def));
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[InterpolationDurationIndex] = interpolationDuration;
            def[SourceValueIndex] = toInterpolate.rotation;
            def[DestinationValueIndex] = destinationWorldRotation;
            ArrList.Add(ref worldRotationDefs, ref worldRotationDefsCount, def);
            updateManager.Register(this);
            return def;
        }

        public object[] InterpolateWorldRotation(
            Transform toInterpolate,
            Quaternion destinationWorldRotation,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateWorldRotation");
#endif
            object[] def = InterpolateWorldRotation(toInterpolate, destinationWorldRotation, interpolationDuration);
            def[CallbackUdonBehaviourIndex] = callbackInst;
            def[CallbackEventNameIndex] = callbackEventName;
            def[CustomCallbackDataIndex] = customCallbackData;
            return def;
        }

        public bool CancelLocalPositionInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelLocalPositionInterpolation");
#endif
            if (!localPositionDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null;
            return true;
        }

        public bool CancelLocalRotationInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelLocalRotationInterpolation");
#endif
            if (!localRotationDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null;
            return true;
        }

        public bool CancelLocalScaleInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelLocalScaleInterpolation");
#endif
            if (!localScaleDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null;
            return true;
        }

        public bool CancelWorldPositionInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelWorldPositionInterpolation");
#endif
            if (!worldPositionDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null;
            return true;
        }

        public bool CancelWorldRotationInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelWorldRotationInterpolation");
#endif
            if (!worldRotationDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null;
            return true;
        }

        public bool CancelPositionInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelPositionInterpolation");
#endif
            return CancelLocalPositionInterpolation(toInterpolate)
                || CancelWorldPositionInterpolation(toInterpolate);
        }

        public bool CancelRotationInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelRotationInterpolation");
#endif
            return CancelLocalRotationInterpolation(toInterpolate)
                || CancelWorldRotationInterpolation(toInterpolate);
        }
    }
}
