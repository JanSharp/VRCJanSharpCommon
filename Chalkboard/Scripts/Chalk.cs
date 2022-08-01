﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UdonSharpEditor;
#endif

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Chalk : UdonSharpBehaviour
    #if UNITY_EDITOR && !COMPILER_UDONSHARP
        , IOnBuildCallback
    #endif
    {
        [SerializeField] private Transform aimPoint;
        // 0: Default, 4: Water, 8: Interactive, 11: Environment
        [SerializeField] private LayerMask layerMask = (1 << 0) | (1 << 4) | (1 << 8) | (1 << 11);
        [SerializeField] private Color color = Color.white;
        [SerializeField] private Transform indicator;
        [SerializeField] public bool isSponge;
        [SerializeField] [HideInInspector] private UpdateManager updateManager;
        [SerializeField] [HideInInspector] private ChalkboardManager chalkboardManager;
        [HideInInspector] public int chalkId;
        // for UpdateManager
        private int customUpdateInternalIndex;
        private bool holding;
        private bool hasPrev;
        private int prevX;
        private int prevY;

        // NOTE: the indexes of the 4 corners for hte chalk are still hard coded,
        // but changing the chalk size isn't really supported anyway.
        // 5x5 both looks the best and is pretty balanced in terms of performance
        private const int ChalkSize = 5;
        private const int ChalkLineDrawingFrequency = 2;
        private const int SpongeSize = 41;
        private const int SpongeLineDrawingFrequency = 8;

        [HideInInspector] [System.NonSerialized] public Color[] colors;
        [HideInInspector] [System.NonSerialized] public int lineDrawingFrequency;
        [HideInInspector] [System.NonSerialized] public int size;
        [HideInInspector] [System.NonSerialized] public int halfSize;

        private const int ActionBitCount = 21;
        private const int AxisBitCount = 10;
        private const ulong PointHasPrev = 0x100000UL;
        private const ulong ActionBits = 0x1fffffUL;
        [UdonSynced]
        private ulong syncedActions;
        private int[] pointsStage = new int[4];
        private int pointsStageCount;
        private int pointsStageStartIndex;
        private const int IntPointHasPrev = 0x100000;
        private const int IntActionBits = 0x1fffff;
        private const int IntUnusedAction = 0; // x + y
        private const int IntSwitchToBoardY = 1; // just y
        private const int IntAxisBits = 0x3ff;

        private const float LateJoinerSyncDelay = 10f; // TODO: set this higher for the real world
        private float lastTimeAPlayerJoined;
        private Chalkboard lastSyncedChalkboard;
        private Chalkboard chalkboard;
        private Texture2D texture;

        #if UNITY_EDITOR && !COMPILER_UDONSHARP
        [InitializeOnLoad]
        public static class OnBuildRegister
        {
            static OnBuildRegister() => JanSharp.OnBuildUtil.RegisterType<Chalk>();
        }
        bool IOnBuildCallback.OnBuild()
        {
            updateManager = GameObject.Find("/UpdateManager")?.GetUdonSharpComponent<UpdateManager>();
            if (updateManager == null)
                Debug.LogError("Chalk requires a GameObject that must be at the root of the scene"
                        + " with the exact name 'UpdateManager' which has the 'UpdateManager' UdonBehaviour.",
                    UdonSharpEditorUtility.GetBackingUdonBehaviour(this));

            chalkboardManager = GameObject.Find("/ChalkboardManager")?.GetUdonSharpComponent<ChalkboardManager>();
            chalkId = chalkboardManager?.GetChalkId(this) ?? -1;
            if (chalkboardManager == null)
                Debug.LogError("Chalk requires a GameObject that must be at the root of the scene"
                        + " with the exact name 'ChalkboardManager' which has the 'ChalkboardManager' UdonBehaviour.",
                    UdonSharpEditorUtility.GetBackingUdonBehaviour(this));

            this.ApplyProxyModifications();
            // EditorUtility.SetDirty(UdonSharpEditorUtility.GetBackingUdonBehaviour(this));
            return updateManager != null && chalkboardManager != null;
        }
        #endif

        private void Start()
        {
            if (isSponge)
            {
                lineDrawingFrequency = SpongeLineDrawingFrequency;
                size = SpongeSize;
            }
            else
            {
                lineDrawingFrequency = ChalkLineDrawingFrequency;
                size = ChalkSize;
            }
            halfSize = size / 2;
            colors = new Color[size * size];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = color;
        }

        public override void OnPickup()
        {
            updateManager.Register(this);
        }

        public override void OnDrop()
        {
            indicator.gameObject.SetActive(false);
            updateManager.Deregister(this);
            // because I don't trust VRC
            holding = false;
            hasPrev = false;
        }

        public override void OnPickupUseDown()
        {
            holding = true;
        }

        public override void OnPickupUseUp()
        {
            holding = false;
            hasPrev = false;
        }

        public void CustomUpdate()
        {
            RaycastHit hit;
            if (Physics.Raycast(aimPoint.position, aimPoint.forward, out hit, 15f, layerMask.value) && hit.transform != null)
            {
                if (chalkboard == null || hit.transform != chalkboard.transform)
                {
                    hasPrev = false;
                    chalkboard = hit.transform.GetComponent<Chalkboard>();
                    if (chalkboard == null)
                    {
                        indicator.gameObject.SetActive(false);
                        return;
                    }
                    texture = chalkboard.texture;
                }
                indicator.gameObject.SetActive(true);
                indicator.SetPositionAndRotation(hit.point, Quaternion.LookRotation(chalkboard.bottomLeft.forward, chalkboard.bottomLeft.up));
                indicator.localScale = isSponge ? chalkboard.spongeScale : chalkboard.chalkScale;
                if (!holding)
                    return;
                var width = texture.width;
                var height = texture.height;
                var localHitPos = chalkboard.transform.InverseTransformPoint(hit.point);
                var blPos = chalkboard.bottomLeft.localPosition;
                var trPos = chalkboard.topRight.localPosition;
                int x = (int)Mathf.Clamp(Mathf.Abs((localHitPos.x - blPos.x) / (trPos.x - blPos.x)) * width, halfSize, width - halfSize - 1);
                int y = (int)Mathf.Clamp(Mathf.Abs((localHitPos.y - blPos.y) / (trPos.y - blPos.y)) * height, halfSize, height - halfSize - 1);
                if (hasPrev && (Mathf.Abs(x - prevX) + Mathf.Abs(y - prevY)) <= 2) // didn't draw more than 2 pixels from prev point? => ignore
                    return;
                AddPointToSyncedPoints(x, y);
                DrawFromPrevTo(x, y);
                chalkboard.UpdateTextureFast();
            }
            else
            {
                indicator.gameObject.SetActive(false);
            }
        }

        private void DrawFromPrevTo(int toX, int toY)
        {
            if (hasPrev)
                lastSyncedChalkboard.DrawLine(this, prevX, prevY, toX, toY);
            else
                lastSyncedChalkboard.DrawPoint(this, toX, toY);
            hasPrev = true;
            prevX = toX;
            prevY = toY;
        }

        private void AddPointToSyncedPoints(int x, int y)
        {
            bool changedBoard = chalkboard != lastSyncedChalkboard || (lastTimeAPlayerJoined + LateJoinerSyncDelay) > Time.time;
            if ((changedBoard ? pointsStageCount + 1 : pointsStageCount) >= pointsStage.Length)
            {
                var newPointsStage = new int[pointsStageCount * 2];
                for (int i = 0; i < pointsStage.Length; i++)
                    newPointsStage[i] = pointsStage[(i + pointsStageStartIndex) % pointsStage.Length];
                pointsStage = newPointsStage;
                pointsStageStartIndex = 0;
            }
            if (changedBoard)
            {
                lastSyncedChalkboard = chalkboard;
                Debug.Log($"<dlt> adding switch to board id: {chalkboard.boardId}");
                // y == 1 is an invalid point, so it means "switch to board [x]" instead
                pointsStage[(pointsStageStartIndex + (pointsStageCount++)) % pointsStage.Length]
                    = chalkboard.boardId | (IntSwitchToBoardY << AxisBitCount);
            }
            Debug.Log($"<dlt> adding point x: {x}, y: {y}, hasPrev: {hasPrev}");
            pointsStage[(pointsStageStartIndex + (pointsStageCount++)) % pointsStage.Length]
                = x | (y << AxisBitCount) | (hasPrev ? IntPointHasPrev : 0);
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            RequestSerialization();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            // have to do both to handle the chalk being used _right now_ as well as at some random point in the future
            lastSyncedChalkboard = null;
            lastTimeAPlayerJoined = Time.time;
        }

        public override void OnPreSerialization()
        {
            Debug.Log($"<dlt> sending {System.Math.Min(pointsStageCount, 3)} actions");
            syncedActions = 0UL;
            if (pointsStageCount == 0)
                return;
            for (int i = 0; i < System.Math.Min(pointsStageCount, 3); i++)
            {
                int stageIndex = (pointsStageStartIndex + i) % pointsStage.Length;
                syncedActions |= ((ulong)(pointsStage[stageIndex] & 0x1fffffU)) << (i * ActionBitCount);
            }

            if (pointsStageCount <= 3)
            {
                pointsStageCount = 0;
                pointsStageStartIndex = 0;
            }
            else
            {
                pointsStageCount -= 3;
                pointsStageStartIndex = (pointsStageStartIndex + 3) % pointsStage.Length;
                SendCustomEventDelayedFrames(nameof(RequestSerializationDelayed), 1);
            }
        }

        public void RequestSerializationDelayed() => RequestSerialization();

        public override void OnPostSerialization(SerializationResult result)
        {
            Debug.Log($"<dlt> on post: success: {result.success}, byteCount: {result.byteCount}");
        }

        public override void OnDeserialization()
        {
            bool doUpdateTexture = false;
            for (int i = 0; i < 3; i++)
            {
                int point = (int)((syncedActions >> (i * ActionBitCount)) & ActionBits);
                if (point == IntUnusedAction)
                    break;
                doUpdateTexture = true;
                int x = point & IntAxisBits;
                int y = (point >> AxisBitCount) & IntAxisBits;
                if (y == IntSwitchToBoardY)
                {
                    Debug.Log($"<dlt> received switch to board id: {x}");
                    if (i != 0 && lastSyncedChalkboard != null) // update the previous texture before switching
                        lastSyncedChalkboard.UpdateTextureSlow();
                    lastSyncedChalkboard = chalkboardManager.chalkboards[x];
                    texture = lastSyncedChalkboard.texture;
                }
                else
                {
                    if ((point & IntPointHasPrev) == 0)
                        hasPrev = false;
                    Debug.Log($"<dlt> received point x: {x}, y: {y} hasPrev: {((point & IntPointHasPrev) != 0)}");
                    if (lastSyncedChalkboard == null)
                        Debug.Log($"<dlt> received point before receiving any switch to a board?!");
                    else
                        DrawFromPrevTo(x, y);
                }
            }
            if (doUpdateTexture && lastSyncedChalkboard != null) // null check just in case there is some edge case with wrong order of packets
                lastSyncedChalkboard.UpdateTextureSlow();
        }
    }
}
