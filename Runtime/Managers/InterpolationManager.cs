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

        #region AddingPositionDefs

        /// <summary>
        /// <para>Uses <see cref="currentToInterpolate"/>.</para>
        /// <para>Writes to <see cref="currentDef"/>.</para>
        /// </summary>
        private void AddPositionInterpolationDef()
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  AddPositionInterpolationDef");
#endif
            DataToken keyToken = currentToInterpolate;
            if (positionDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                currentDef = (object[])defToken.Reference;
                CallCallback();
                currentDef[CallbackUdonBehaviourIndex] = null;
            }
            else
            {
                currentDef = new object[DefinitionSize];
                positionDefsLut.Add(keyToken, new DataToken(currentDef));
                ArrList.Add(ref positionDefs, ref positionDefsCount, currentDef);
                updateManager.Register(this);
            }
            currentDef[ToInterpolateIndex] = currentToInterpolate;
            currentDef[StartTimeIndex] = Time.time;
        }

        public object[] InterpolateLocalPosition(
            Transform toInterpolate,
            Vector3 destinationLocalPosition,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateLocalPosition");
#endif
            currentToInterpolate = toInterpolate;
            AddPositionInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.localPosition;
            currentDef[DestinationValueIndex] = destinationLocalPosition;
            currentDef[InterpolationEventNameIndex] = nameof(LerpLocalPositionHandler);
            return currentDef;
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
            currentToInterpolate = toInterpolate;
            AddPositionInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.localPosition;
            currentDef[DestinationValueIndex] = destinationLocalPosition;
            currentDef[InterpolationEventNameIndex] = nameof(LerpLocalPositionHandler);
            currentDef[CallbackUdonBehaviourIndex] = callbackInst;
            currentDef[CallbackEventNameIndex] = callbackEventName;
            currentDef[CustomCallbackDataIndex] = customCallbackData;
            return currentDef;
        }

        public object[] InterpolateWorldPosition(
            Transform toInterpolate,
            Vector3 destinationWorldPosition,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateWorldPosition");
#endif
            currentToInterpolate = toInterpolate;
            AddPositionInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.position;
            currentDef[DestinationValueIndex] = destinationWorldPosition;
            currentDef[InterpolationEventNameIndex] = nameof(LerpWorldPositionHandler);
            return currentDef;
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
            currentToInterpolate = toInterpolate;
            AddPositionInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.position;
            currentDef[DestinationValueIndex] = destinationWorldPosition;
            currentDef[InterpolationEventNameIndex] = nameof(LerpWorldPositionHandler);
            currentDef[CallbackUdonBehaviourIndex] = callbackInst;
            currentDef[CallbackEventNameIndex] = callbackEventName;
            currentDef[CustomCallbackDataIndex] = customCallbackData;
            return currentDef;
        }

        #endregion

        #region AddingRotationDefs

        /// <summary>
        /// <para>Uses <see cref="currentToInterpolate"/>.</para>
        /// <para>Writes to <see cref="currentDef"/>.</para>
        /// </summary>
        private void AddRotationInterpolationDef()
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  AddRotationInterpolationDef");
#endif
            DataToken keyToken = currentToInterpolate;
            if (rotationDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                currentDef = (object[])defToken.Reference;
                CallCallback();
                currentDef[CallbackUdonBehaviourIndex] = null;
            }
            else
            {
                currentDef = new object[DefinitionSize];
                rotationDefsLut.Add(keyToken, new DataToken(currentDef));
                ArrList.Add(ref rotationDefs, ref rotationDefsCount, currentDef);
                updateManager.Register(this);
            }
            currentDef[ToInterpolateIndex] = currentToInterpolate;
            currentDef[StartTimeIndex] = Time.time;
        }

        public object[] InterpolateLocalRotation(
            Transform toInterpolate,
            Quaternion destinationLocalRotation,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateLocalRotation");
#endif
            currentToInterpolate = toInterpolate;
            AddRotationInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.localRotation;
            currentDef[DestinationValueIndex] = destinationLocalRotation;
            currentDef[InterpolationEventNameIndex] = nameof(LerpLocalRotationHandler);
            return currentDef;
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
            currentToInterpolate = toInterpolate;
            AddRotationInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.localRotation;
            currentDef[DestinationValueIndex] = destinationLocalRotation;
            currentDef[InterpolationEventNameIndex] = nameof(LerpLocalRotationHandler);
            currentDef[CallbackUdonBehaviourIndex] = callbackInst;
            currentDef[CallbackEventNameIndex] = callbackEventName;
            currentDef[CustomCallbackDataIndex] = customCallbackData;
            return currentDef;
        }

        public object[] InterpolateWorldRotation(
            Transform toInterpolate,
            Quaternion destinationWorldRotation,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateWorldRotation");
#endif
            currentToInterpolate = toInterpolate;
            AddRotationInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.rotation;
            currentDef[DestinationValueIndex] = destinationWorldRotation;
            currentDef[InterpolationEventNameIndex] = nameof(LerpWorldRotationHandler);
            return currentDef;
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
            currentToInterpolate = toInterpolate;
            AddRotationInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.rotation;
            currentDef[DestinationValueIndex] = destinationWorldRotation;
            currentDef[InterpolationEventNameIndex] = nameof(LerpWorldRotationHandler);
            currentDef[CallbackUdonBehaviourIndex] = callbackInst;
            currentDef[CallbackEventNameIndex] = callbackEventName;
            currentDef[CustomCallbackDataIndex] = customCallbackData;
            return currentDef;
        }

        #endregion

        #region AddingScaleDefs

        /// <summary>
        /// <para>Uses <see cref="currentToInterpolate"/>.</para>
        /// <para>Writes to <see cref="currentDef"/>.</para>
        /// </summary>
        private void AddScaleInterpolationDef()
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  AddScaleInterpolationDef");
#endif
            DataToken keyToken = currentToInterpolate;
            if (scaleDefsLut.TryGetValue(keyToken, out DataToken defToken))
            {
                currentDef = (object[])defToken.Reference;
                CallCallback();
                currentDef[CallbackUdonBehaviourIndex] = null;
            }
            else
            {
                currentDef = new object[DefinitionSize];
                scaleDefsLut.Add(keyToken, new DataToken(currentDef));
                ArrList.Add(ref scaleDefs, ref scaleDefsCount, currentDef);
                updateManager.Register(this);
            }
            currentDef[ToInterpolateIndex] = currentToInterpolate;
            currentDef[StartTimeIndex] = Time.time;
        }

        public object[] InterpolateLocalScale(
            Transform toInterpolate,
            Vector3 destinationLocalScale,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  InterpolateLocalScale");
#endif
            currentToInterpolate = toInterpolate;
            AddScaleInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.localScale;
            currentDef[DestinationValueIndex] = destinationLocalScale;
            currentDef[InterpolationEventNameIndex] = nameof(LerpLocalScaleHandler);
            return currentDef;
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
            currentToInterpolate = toInterpolate;
            AddScaleInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.localScale;
            currentDef[DestinationValueIndex] = destinationLocalScale;
            currentDef[InterpolationEventNameIndex] = nameof(LerpLocalScaleHandler);
            currentDef[CallbackUdonBehaviourIndex] = callbackInst;
            currentDef[CallbackEventNameIndex] = callbackEventName;
            currentDef[CustomCallbackDataIndex] = customCallbackData;
            return currentDef;
        }

        #endregion

        #region Cancelling

        public bool CancelPositionInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelPositionInterpolation");
#endif
            if (!positionDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null; // Update logic handles callback and removing.
            return true;
        }

        public bool CancelRotationInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelRotationInterpolation");
#endif
            if (!rotationDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null; // Update logic handles callback and removing.
            return true;
        }

        public bool CancelScaleInterpolation(Transform toInterpolate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  CancelScaleInterpolation");
#endif
            if (!scaleDefsLut.Remove(toInterpolate, out DataToken defToken))
                return false;
            ((object[])defToken.Reference)[ToInterpolateIndex] = null; // Update logic handles callback and removing.
            return true;
        }

        #endregion
    }
}
