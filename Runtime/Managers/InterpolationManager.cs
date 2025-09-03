using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

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
        private const int InterpolationEventNameIndex = 5;
        private const int CallbackUdonBehaviourIndex = 6;
        private const int CallbackEventNameIndex = 7;
        private const int CustomCallbackDataIndex = 8;
        private const int DefinitionSize = 9;

        private object[][] positionDefs = new object[ArrList.MinCapacity][];
        private int positionDefsCount = 0;
        private DataDictionary positionDefsLut = new DataDictionary();

        private object[][] rotationDefs = new object[ArrList.MinCapacity][];
        private int rotationDefsCount = 0;
        private DataDictionary rotationDefsLut = new DataDictionary();

        private object[][] scaleDefs = new object[ArrList.MinCapacity][];
        private int scaleDefsCount = 0;
        private DataDictionary scaleDefsLut = new DataDictionary();

        private Transform finishedTransform;
        public Transform FinishedTransform => finishedTransform;
        private object customCallbackData;
        public object CustomCallbackData => customCallbackData;

        private float timeTime;

        private object[] currentDef;
        private Transform currentToInterpolate;
        private float currentT;

        private void UpdatePositionDefs()
        {
            for (int i = positionDefsCount - 1; i >= 0; i--)
            {
                currentDef = positionDefs[i];
                currentToInterpolate = (Transform)currentDef[ToInterpolateIndex];
                if (currentToInterpolate == null)
                {
                    positionDefs[i] = positionDefs[--positionDefsCount];
                    // Cannot remove from the lut using 'null' as the key - that does nothing - so cannot clean up.
                    CallCallback();
                    continue;
                }
                float startTime = (float)currentDef[StartTimeIndex];
                float interpolationDuration = (float)currentDef[InterpolationDurationIndex];
                currentT = (timeTime - startTime) / interpolationDuration;
                if (currentT >= 1f)
                {
                    currentToInterpolate.localPosition = (Vector3)currentDef[DestinationValueIndex];
                    positionDefs[i] = positionDefs[--positionDefsCount];
                    positionDefsLut.Remove(currentToInterpolate);
                    CallCallback();
                    continue;
                }
                SendCustomEvent((string)currentDef[InterpolationEventNameIndex]);
            }
        }

        private void UpdateRotationDefs()
        {
            for (int i = rotationDefsCount - 1; i >= 0; i--)
            {
                currentDef = rotationDefs[i];
                currentToInterpolate = (Transform)currentDef[ToInterpolateIndex];
                if (currentToInterpolate == null)
                {
                    rotationDefs[i] = rotationDefs[--rotationDefsCount];
                    // Cannot remove from the lut using 'null' as the key - that does nothing - so cannot clean up.
                    CallCallback();
                    continue;
                }
                float startTime = (float)currentDef[StartTimeIndex];
                float interpolationDuration = (float)currentDef[InterpolationDurationIndex];
                currentT = (timeTime - startTime) / interpolationDuration;
                if (currentT >= 1f)
                {
                    currentToInterpolate.localRotation = (Quaternion)currentDef[DestinationValueIndex];
                    rotationDefs[i] = rotationDefs[--rotationDefsCount];
                    rotationDefsLut.Remove(currentToInterpolate);
                    CallCallback();
                    continue;
                }
                SendCustomEvent((string)currentDef[InterpolationEventNameIndex]);
            }
        }

        private void UpdateScaleDefs()
        {
            for (int i = scaleDefsCount - 1; i >= 0; i--)
            {
                currentDef = scaleDefs[i];
                currentToInterpolate = (Transform)currentDef[ToInterpolateIndex];
                if (currentToInterpolate == null)
                {
                    scaleDefs[i] = scaleDefs[--scaleDefsCount];
                    // Cannot remove from the lut using 'null' as the key - that does nothing - so cannot clean up.
                    CallCallback();
                    continue;
                }
                float startTime = (float)currentDef[StartTimeIndex];
                float interpolationDuration = (float)currentDef[InterpolationDurationIndex];
                currentT = (timeTime - startTime) / interpolationDuration;
                if (currentT >= 1f)
                {
                    currentToInterpolate.localScale = (Vector3)currentDef[DestinationValueIndex];
                    scaleDefs[i] = scaleDefs[--scaleDefsCount];
                    scaleDefsLut.Remove(currentToInterpolate);
                    CallCallback();
                    continue;
                }
                SendCustomEvent((string)currentDef[InterpolationEventNameIndex]);
            }
        }

        public void LerpLocalPositionHandler()
        {
            currentToInterpolate.localPosition = Vector3.Lerp(
                (Vector3)currentDef[SourceValueIndex],
                (Vector3)currentDef[DestinationValueIndex],
                currentT);
        }

        public void LerpWorldPositionHandler()
        {
            currentToInterpolate.position = Vector3.Lerp(
                (Vector3)currentDef[SourceValueIndex],
                (Vector3)currentDef[DestinationValueIndex],
                currentT);
        }

        public void LerpLocalRotationHandler()
        {
            currentToInterpolate.localRotation = Quaternion.Lerp(
                (Quaternion)currentDef[SourceValueIndex],
                (Quaternion)currentDef[DestinationValueIndex],
                currentT);
        }

        public void LerpWorldRotationHandler()
        {
            currentToInterpolate.rotation = Quaternion.Lerp(
                (Quaternion)currentDef[SourceValueIndex],
                (Quaternion)currentDef[DestinationValueIndex],
                currentT);
        }

        public void LerpLocalScaleHandler()
        {
            currentToInterpolate.localScale = Vector3.Lerp(
                (Vector3)currentDef[SourceValueIndex],
                (Vector3)currentDef[DestinationValueIndex],
                currentT);
        }

        /// <summary>
        /// <para>Uses <see cref="currentDef"/> and <see cref="currentToInterpolate"/></para>
        /// </summary>
        private void CallCallback()
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CallCallback");
#endif
            UdonSharpBehaviour callbackInst = (UdonSharpBehaviour)currentDef[CallbackUdonBehaviourIndex];
            if (callbackInst != null)
            {
                string callbackEventName = (string)currentDef[CallbackEventNameIndex];
                customCallbackData = currentDef[CustomCallbackDataIndex];
                finishedTransform = currentToInterpolate;
                callbackInst.SendCustomEvent(callbackEventName);
                finishedTransform = null;
                customCallbackData = null;
            }
        }

        public void CustomUpdate()
        {
            timeTime = Time.time;
            bool doDeregister = true;
            if (positionDefsCount != 0)
            {
                UpdatePositionDefs();
                doDeregister = false;
            }
            if (rotationDefsCount != 0)
            {
                UpdateRotationDefs();
                doDeregister = false;
            }
            if (scaleDefsCount != 0)
            {
                UpdateScaleDefs();
                doDeregister = false;
            }

            if (doDeregister)
            {
#if JAN_SHARP_COMMON_DEBUG
                Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CustomUpdate (inner) - positionDefsCount: {positionDefsCount}, rotationDefsCount: {rotationDefsCount}, scaleDefsCount: {scaleDefsCount}");
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
            object[] def;
            DataToken keyToken = toInterpolate;
            if (positionDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                def = (object[])defToken.Reference;
                currentDef = def;
                currentToInterpolate = toInterpolate;
                CallCallback();
            }
            else
            {
                def = new object[DefinitionSize];
                positionDefsLut.Add(keyToken, new DataToken(def));
                ArrList.Add(ref positionDefs, ref positionDefsCount, def);
                updateManager.Register(this);
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[InterpolationDurationIndex] = interpolationDuration;
            def[SourceValueIndex] = toInterpolate.localPosition;
            def[DestinationValueIndex] = destinationLocalPosition;
            def[InterpolationEventNameIndex] = nameof(LerpLocalPositionHandler);
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
            object[] def;
            DataToken keyToken = toInterpolate;
            if (rotationDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                def = (object[])defToken.Reference;
                currentDef = def;
                currentToInterpolate = toInterpolate;
                CallCallback();
            }
            else
            {
                def = new object[DefinitionSize];
                rotationDefsLut.Add(keyToken, new DataToken(def));
                ArrList.Add(ref rotationDefs, ref rotationDefsCount, def);
                updateManager.Register(this);
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[InterpolationDurationIndex] = interpolationDuration;
            def[SourceValueIndex] = toInterpolate.localRotation;
            def[DestinationValueIndex] = destinationLocalRotation;
            def[InterpolationEventNameIndex] = nameof(LerpLocalRotationHandler);
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
            if (scaleDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                def = (object[])defToken.Reference;
                currentDef = def;
                currentToInterpolate = toInterpolate;
                CallCallback();
            }
            else
            {
                def = new object[DefinitionSize];
                scaleDefsLut.Add(keyToken, new DataToken(def));
                ArrList.Add(ref scaleDefs, ref scaleDefsCount, def);
                updateManager.Register(this);
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[InterpolationDurationIndex] = interpolationDuration;
            def[SourceValueIndex] = toInterpolate.localScale;
            def[DestinationValueIndex] = destinationLocalScale;
            def[InterpolationEventNameIndex] = nameof(LerpLocalScaleHandler);
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
            object[] def;
            DataToken keyToken = toInterpolate;
            if (positionDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                def = (object[])defToken.Reference;
                currentDef = def;
                currentToInterpolate = toInterpolate;
                CallCallback();
            }
            else
            {
                def = new object[DefinitionSize];
                positionDefsLut.Add(keyToken, new DataToken(def));
                ArrList.Add(ref positionDefs, ref positionDefsCount, def);
                updateManager.Register(this);
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[InterpolationDurationIndex] = interpolationDuration;
            def[SourceValueIndex] = toInterpolate.position;
            def[DestinationValueIndex] = destinationWorldPosition;
            def[InterpolationEventNameIndex] = nameof(LerpWorldPositionHandler);
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
            object[] def;
            DataToken keyToken = toInterpolate;
            if (rotationDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                def = (object[])defToken.Reference;
                currentDef = def;
                currentToInterpolate = toInterpolate;
                CallCallback();
            }
            else
            {
                def = new object[DefinitionSize];
                rotationDefsLut.Add(keyToken, new DataToken(def));
                ArrList.Add(ref rotationDefs, ref rotationDefsCount, def);
                updateManager.Register(this);
            }
            def[ToInterpolateIndex] = toInterpolate;
            def[StartTimeIndex] = Time.time;
            def[InterpolationDurationIndex] = interpolationDuration;
            def[SourceValueIndex] = toInterpolate.rotation;
            def[DestinationValueIndex] = destinationWorldRotation;
            def[InterpolationEventNameIndex] = nameof(LerpWorldRotationHandler);
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

        public bool CancelPositionInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelPositionInterpolation");
#endif
            if (!positionDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null;
            return true;
        }

        public bool CancelRotationInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelRotationInterpolation");
#endif
            if (!rotationDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null;
            return true;
        }

        public bool CancelScaleInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelScaleInterpolation");
#endif
            if (!scaleDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null;
            return true;
        }
    }
}
