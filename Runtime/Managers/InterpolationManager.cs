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
        /// <summary>
        /// <para>Same index as <see cref="SourceValueIndex"/>, as it happens to literally be the source value
        /// when using hermite curves.</para>
        /// </summary>
        private const int HermitePrecomputed0Index = 3;
        private const int HermitePrecomputed1Index = 5;
        private const int HermitePrecomputed2Index = 6;
        private const int HermitePrecomputed3Index = 7;
        private const int InterpolationEventNameIndex = 8;
        private const int CallbackUdonBehaviourIndex = 9;
        private const int CallbackEventNameIndex = 10;
        private const int CustomCallbackDataIndex = 11;
        private const int DefinitionSize = 12;

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

        public void HermiteCurveLocalPositionHandler()
        {
            currentToInterpolate.localPosition
                = (Vector3)currentDef[HermitePrecomputed0Index]
                + currentT * (Vector3)currentDef[HermitePrecomputed1Index]
                + currentT * currentT * (Vector3)currentDef[HermitePrecomputed2Index]
                + currentT * currentT * currentT * (Vector3)currentDef[HermitePrecomputed3Index];
        }

        public void HermiteCurveWorldPositionHandler()
        {
            currentToInterpolate.position
                = (Vector3)currentDef[HermitePrecomputed0Index]
                + currentT * (Vector3)currentDef[HermitePrecomputed1Index]
                + currentT * currentT * (Vector3)currentDef[HermitePrecomputed2Index]
                + currentT * currentT * currentT * (Vector3)currentDef[HermitePrecomputed3Index];
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

        public void HermiteCurveLocalScaleHandler()
        {
            currentToInterpolate.localScale
                = (Vector3)currentDef[HermitePrecomputed0Index]
                + currentT * (Vector3)currentDef[HermitePrecomputed1Index]
                + currentT * currentT * (Vector3)currentDef[HermitePrecomputed2Index]
                + currentT * currentT * currentT * (Vector3)currentDef[HermitePrecomputed3Index];
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

        public object[] LerpLocalPosition(
            Transform toInterpolate,
            Vector3 destinationLocalPosition,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  LerpLocalPosition");
#endif
            currentToInterpolate = toInterpolate;
            AddPositionInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.localPosition;
            currentDef[DestinationValueIndex] = destinationLocalPosition;
            currentDef[InterpolationEventNameIndex] = nameof(LerpLocalPositionHandler);
            return currentDef;
        }

        public object[] LerpLocalPosition(
            Transform toInterpolate,
            Vector3 destinationLocalPosition,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  LerpLocalPosition");
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

        public object[] LerpWorldPosition(
            Transform toInterpolate,
            Vector3 destinationWorldPosition,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  LerpWorldPosition");
#endif
            currentToInterpolate = toInterpolate;
            AddPositionInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.position;
            currentDef[DestinationValueIndex] = destinationWorldPosition;
            currentDef[InterpolationEventNameIndex] = nameof(LerpWorldPositionHandler);
            return currentDef;
        }

        public object[] LerpWorldPosition(
            Transform toInterpolate,
            Vector3 destinationWorldPosition,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  LerpWorldPosition");
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

        /// <summary>
        /// <para>Uses the current <see cref="Transform.localPosition"/> of <paramref name="toInterpolate"/>
        /// as the sourceLocalPosition.</para>
        /// </summary>
        public object[] HermiteCurveLocalPosition(
            Transform toInterpolate,
            Vector3 sourceLocalVelocity,
            Vector3 destinationLocalPosition,
            Vector3 destinationLocalVelocity,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  HermiteCurveLocalPosition");
#endif
            currentToInterpolate = toInterpolate;
            AddPositionInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            Vector3 sourceLocalPosition = toInterpolate.localPosition;
            currentDef[DestinationValueIndex] = destinationLocalPosition;
            // Matrix4x4 HermiteCharacteristic = new(
            //     new Vector4(1f, 0f, 0f, 0f),
            //     new Vector4(0f, 1f, 0f, 0f),
            //     new Vector4(-3f, -2f, 3f, -1f),
            //     new Vector4(2f, 1f, -2f, 1f));
            currentDef[HermitePrecomputed0Index] = sourceLocalPosition;
            currentDef[HermitePrecomputed1Index] = sourceLocalVelocity;
            currentDef[HermitePrecomputed2Index] = -3f * sourceLocalPosition
                                                 + -2f * sourceLocalVelocity
                                                 + 3f * destinationLocalPosition
                                                 - destinationLocalVelocity;
            currentDef[HermitePrecomputed3Index] = 2f * sourceLocalPosition
                                                 + sourceLocalVelocity
                                                 + -2f * destinationLocalPosition
                                                 + destinationLocalVelocity;
            currentDef[InterpolationEventNameIndex] = nameof(HermiteCurveLocalPositionHandler);
            return currentDef;
        }

        /// <summary>
        /// <para>Uses the current <see cref="Transform.localPosition"/> of <paramref name="toInterpolate"/>
        /// as the sourceLocalPosition.</para>
        /// </summary>
        public object[] HermiteCurveLocalPosition(
            Transform toInterpolate,
            Vector3 sourceLocalVelocity,
            Vector3 destinationLocalPosition,
            Vector3 destinationLocalVelocity,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  HermiteCurveLocalPosition");
#endif
            currentToInterpolate = toInterpolate;
            AddPositionInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            Vector3 sourceLocalPosition = toInterpolate.localPosition;
            currentDef[DestinationValueIndex] = destinationLocalPosition;
            // Matrix4x4 HermiteCharacteristic = new(
            //     new Vector4(1f, 0f, 0f, 0f),
            //     new Vector4(0f, 1f, 0f, 0f),
            //     new Vector4(-3f, -2f, 3f, -1f),
            //     new Vector4(2f, 1f, -2f, 1f));
            currentDef[HermitePrecomputed0Index] = sourceLocalPosition;
            currentDef[HermitePrecomputed1Index] = sourceLocalVelocity;
            currentDef[HermitePrecomputed2Index] = -3f * sourceLocalPosition
                                                 + -2f * sourceLocalVelocity
                                                 + 3f * destinationLocalPosition
                                                 - destinationLocalVelocity;
            currentDef[HermitePrecomputed3Index] = 2f * sourceLocalPosition
                                                 + sourceLocalVelocity
                                                 + -2f * destinationLocalPosition
                                                 + destinationLocalVelocity;
            currentDef[InterpolationEventNameIndex] = nameof(HermiteCurveLocalPositionHandler);
            currentDef[CallbackUdonBehaviourIndex] = callbackInst;
            currentDef[CallbackEventNameIndex] = callbackEventName;
            currentDef[CustomCallbackDataIndex] = customCallbackData;
            return currentDef;
        }

        /// <summary>
        /// <para>Uses the current <see cref="Transform.position"/> of <paramref name="toInterpolate"/> as the
        /// sourceWorldPosition.</para>
        /// </summary>
        public object[] HermiteCurveWorldPosition(
            Transform toInterpolate,
            Vector3 sourceWorldVelocity,
            Vector3 destinationWorldPosition,
            Vector3 destinationWorldVelocity,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  HermiteCurveWorldPosition");
#endif
            currentToInterpolate = toInterpolate;
            AddPositionInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            Vector3 sourceWorldPosition = toInterpolate.position;
            currentDef[DestinationValueIndex] = destinationWorldPosition;
            // Matrix4x4 HermiteCharacteristic = new(
            //     new Vector4(1f, 0f, 0f, 0f),
            //     new Vector4(0f, 1f, 0f, 0f),
            //     new Vector4(-3f, -2f, 3f, -1f),
            //     new Vector4(2f, 1f, -2f, 1f));
            currentDef[HermitePrecomputed0Index] = sourceWorldPosition;
            currentDef[HermitePrecomputed1Index] = sourceWorldVelocity;
            currentDef[HermitePrecomputed2Index] = -3f * sourceWorldPosition
                                                 + -2f * sourceWorldVelocity
                                                 + 3f * destinationWorldPosition
                                                 - destinationWorldVelocity;
            currentDef[HermitePrecomputed3Index] = 2f * sourceWorldPosition
                                                 + sourceWorldVelocity
                                                 + -2f * destinationWorldPosition
                                                 + destinationWorldVelocity;
            currentDef[InterpolationEventNameIndex] = nameof(HermiteCurveWorldPositionHandler);
            return currentDef;
        }

        /// <summary>
        /// <para>Uses the current <see cref="Transform.position"/> of <paramref name="toInterpolate"/> as the
        /// sourceWorldPosition.</para>
        /// </summary>
        public object[] HermiteCurveWorldPosition(
            Transform toInterpolate,
            Vector3 sourceWorldVelocity,
            Vector3 destinationWorldPosition,
            Vector3 destinationWorldVelocity,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  HermiteCurveWorldPosition");
#endif
            currentToInterpolate = toInterpolate;
            AddPositionInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            Vector3 sourceWorldPosition = toInterpolate.position;
            currentDef[DestinationValueIndex] = destinationWorldPosition;
            // Matrix4x4 HermiteCharacteristic = new(
            //     new Vector4(1f, 0f, 0f, 0f),
            //     new Vector4(0f, 1f, 0f, 0f),
            //     new Vector4(-3f, -2f, 3f, -1f),
            //     new Vector4(2f, 1f, -2f, 1f));
            currentDef[HermitePrecomputed0Index] = sourceWorldPosition;
            currentDef[HermitePrecomputed1Index] = sourceWorldVelocity;
            currentDef[HermitePrecomputed2Index] = -3f * sourceWorldPosition
                                                 + -2f * sourceWorldVelocity
                                                 + 3f * destinationWorldPosition
                                                 - destinationWorldVelocity;
            currentDef[HermitePrecomputed3Index] = 2f * sourceWorldPosition
                                                 + sourceWorldVelocity
                                                 + -2f * destinationWorldPosition
                                                 + destinationWorldVelocity;
            currentDef[InterpolationEventNameIndex] = nameof(HermiteCurveWorldPositionHandler);
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

        public object[] LerpLocalRotation(
            Transform toInterpolate,
            Quaternion destinationLocalRotation,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  LerpLocalRotation");
#endif
            currentToInterpolate = toInterpolate;
            AddRotationInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.localRotation;
            currentDef[DestinationValueIndex] = destinationLocalRotation;
            currentDef[InterpolationEventNameIndex] = nameof(LerpLocalRotationHandler);
            return currentDef;
        }

        public object[] LerpLocalRotation(
            Transform toInterpolate,
            Quaternion destinationLocalRotation,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  LerpLocalRotation");
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

        public object[] LerpWorldRotation(
            Transform toInterpolate,
            Quaternion destinationWorldRotation,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  LerpWorldRotation");
#endif
            currentToInterpolate = toInterpolate;
            AddRotationInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.rotation;
            currentDef[DestinationValueIndex] = destinationWorldRotation;
            currentDef[InterpolationEventNameIndex] = nameof(LerpWorldRotationHandler);
            return currentDef;
        }

        public object[] LerpWorldRotation(
            Transform toInterpolate,
            Quaternion destinationWorldRotation,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  LerpWorldRotation");
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

        public object[] LerpLocalScale(
            Transform toInterpolate,
            Vector3 destinationLocalScale,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  LerpLocalScale");
#endif
            currentToInterpolate = toInterpolate;
            AddScaleInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            currentDef[SourceValueIndex] = toInterpolate.localScale;
            currentDef[DestinationValueIndex] = destinationLocalScale;
            currentDef[InterpolationEventNameIndex] = nameof(LerpLocalScaleHandler);
            return currentDef;
        }

        public object[] LerpLocalScale(
            Transform toInterpolate,
            Vector3 destinationLocalScale,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  LerpLocalScale");
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

        /// <summary>
        /// <para>Uses the current <see cref="Transform.localScale"/> of <paramref name="toInterpolate"/> as
        /// the sourceLocalScale.</para>
        /// </summary>
        public object[] HermiteCurveLocalScale(
            Transform toInterpolate,
            Vector3 sourceLocalVelocity,
            Vector3 destinationLocalScale,
            Vector3 destinationLocalVelocity,
            float interpolationDuration)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  HermiteCurveLocalScale");
#endif
            currentToInterpolate = toInterpolate;
            AddScaleInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            Vector3 sourceLocalScale = toInterpolate.localScale;
            currentDef[DestinationValueIndex] = destinationLocalScale;
            // Matrix4x4 HermiteCharacteristic = new(
            //     new Vector4(1f, 0f, 0f, 0f),
            //     new Vector4(0f, 1f, 0f, 0f),
            //     new Vector4(-3f, -2f, 3f, -1f),
            //     new Vector4(2f, 1f, -2f, 1f));
            currentDef[HermitePrecomputed0Index] = sourceLocalScale;
            currentDef[HermitePrecomputed1Index] = sourceLocalVelocity;
            currentDef[HermitePrecomputed2Index] = -3f * sourceLocalScale
                                                 + -2f * sourceLocalVelocity
                                                 + 3f * destinationLocalScale
                                                 - destinationLocalVelocity;
            currentDef[HermitePrecomputed3Index] = 2f * sourceLocalScale
                                                 + sourceLocalVelocity
                                                 + -2f * destinationLocalScale
                                                 + destinationLocalVelocity;
            currentDef[InterpolationEventNameIndex] = nameof(HermiteCurveLocalScaleHandler);
            return currentDef;
        }

        /// <summary>
        /// <para>Uses the current <see cref="Transform.localScale"/> of <paramref name="toInterpolate"/> as
        /// the sourceLocalScale.</para>
        /// </summary>
        public object[] HermiteCurveLocalScale(
            Transform toInterpolate,
            Vector3 sourceLocalVelocity,
            Vector3 destinationLocalScale,
            Vector3 destinationLocalVelocity,
            float interpolationDuration,
            UdonSharpBehaviour callbackInst,
            string callbackEventName,
            object customCallbackData)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] InterpolationManager  HermiteCurveLocalScale");
#endif
            currentToInterpolate = toInterpolate;
            AddScaleInterpolationDef();
            currentDef[InterpolationDurationIndex] = interpolationDuration;
            Vector3 sourceLocalScale = toInterpolate.localScale;
            currentDef[DestinationValueIndex] = destinationLocalScale;
            // Matrix4x4 HermiteCharacteristic = new(
            //     new Vector4(1f, 0f, 0f, 0f),
            //     new Vector4(0f, 1f, 0f, 0f),
            //     new Vector4(-3f, -2f, 3f, -1f),
            //     new Vector4(2f, 1f, -2f, 1f));
            currentDef[HermitePrecomputed0Index] = sourceLocalScale;
            currentDef[HermitePrecomputed1Index] = sourceLocalVelocity;
            currentDef[HermitePrecomputed2Index] = -3f * sourceLocalScale
                                                 + -2f * sourceLocalVelocity
                                                 + 3f * destinationLocalScale
                                                 - destinationLocalVelocity;
            currentDef[HermitePrecomputed3Index] = 2f * sourceLocalScale
                                                 + sourceLocalVelocity
                                                 + -2f * destinationLocalScale
                                                 + destinationLocalVelocity;
            currentDef[InterpolationEventNameIndex] = nameof(HermiteCurveLocalScaleHandler);
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
