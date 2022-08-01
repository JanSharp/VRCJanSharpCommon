﻿using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UdonSharpEditor;
#endif

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Chalkboard : UdonSharpBehaviour
    #if UNITY_EDITOR && !COMPILER_UDONSHARP
        , IOnBuildCallback
    #endif
    {
        public Transform bottomLeft;
        public Transform topRight;
        public Material material;
        public Slider progressBar;

        [HideInInspector] public int boardId;
        [HideInInspector] public Transform boardParent;
        [HideInInspector] [SerializeField] private ChalkboardManager chalkboardManager;
        [HideInInspector] [System.NonSerialized] public Texture2D texture;
        [HideInInspector] public Vector3 chalkScale;
        [HideInInspector] public Vector3 spongeScale;
        private Color[] initialPixels;
        private const float fastUpdateDelayPerPixel = (1f / 15f) / (1024 * 512);
        private const float slowUpdateDelayPerPixel = (1f / 4f) / (1024 * 512);
        private const float superSlowUpdateDelayPerPixel = 10f / (1024 * 512);
        private float fastUpdateDelay;
        private float slowUpdateDelay;
        private float superSlowUpdateDelay;
        private bool fastUpdating;
        private bool slowUpdating;
        private bool superSlowUpdating;
        private int localUsageCount;
        private const int AttemptToTakeOwnershipInterval = 250;
        private ulong[] allActions;
        private int allActionsCount;
        private ulong currentActions;
        private int currentActionsIndex;
        private const int ActionBitCount = 21;
        private const int AxisBitCount = 10;
        private const ulong MetadataFlag = 0x8000000000000000UL;
        private const ulong ActionCountMetadataFlag = 0x4000000000000000UL;
        private const ulong PointHasPrev = 0x100000UL;
        private const ulong ActionBits = 0x1fffffUL;
        private const int IntPointHasPrev = 0x100000;
        private const int IntUnusedAction = 0; // x + y
        private const int IntSwitchToChalkY = 1; // just y
        private const int IntAxisBits = 0x3ff;
        private Chalk prevChalk;
        private int prevX;
        private int prevY;
        private bool waitingToStartSending;
        private bool firstSend;
        private bool sending;
        [UdonSynced] private ulong syncedActions1;
        [UdonSynced] private ulong syncedActions2;
        [UdonSynced] private ulong syncedActions3;
        [UdonSynced] private ulong syncedActions4;
        private int expectedReceivedActionsCount;
        private int currentReceivedActionIndex;
        private int currentSyncedIndex;
        private int actionsCountRequiredToSync;
        private bool catchingUp;
        private bool catchingUpWithTheQueue;
        private Chalk receivedChalk;
        private int receivedPrevX;
        private int receivedPrevY;
        private const float SyncFrequency = 0.3f;
        private const float LateJoinerSyncDelay = 10f; // TODO: set this higher for the real world
        private ulong[] catchUpQueue;
        private int catchUpQueueCount;
        private int catchUpQueueIndex;
        private bool somebodyIsCatchingUp;

        private void Start()
        {
            allActions = new ulong[64];
            texture = (Texture2D)material.mainTexture;
            // ok so this seems to be using about 9MB for a 1024x512 texture
            // if there was no overhead that should be 2MB
            // note that this is also based on one single test in the editor with 3 boards in the scene
            // however considering this saves us having to loop through all pixels to reset the color
            // well I really can't say it is a good option, but it is an option that allows us to
            // reset the board to the initial state in 2ms instead of 1.5s which it is when looping
            // through all pixels to reset them takes (first a GetPixels call, then the loop, then SetPixels)
            initialPixels = texture.GetPixels();
            fastUpdateDelay = fastUpdateDelayPerPixel * initialPixels.Length;
            slowUpdateDelay = slowUpdateDelayPerPixel * initialPixels.Length;
            superSlowUpdateDelay = superSlowUpdateDelayPerPixel * initialPixels.Length;
        }

        // fast is kept completely separate from slow because when multiple people are drawing
        // we wouldn't want to inconsistently switch between slow and fast updates
        // instead it'll just update a bit quicker 4 times per frame while updating 15 times
        // per frame in general, so a total of ~19 updates. Still irregular but at least
        // it's updating quickly and the logic is simple

        public void UpdateTextureFast()
        {
            if (fastUpdating)
                return;
            fastUpdating = true;
            SendCustomEventDelayedSeconds(nameof(UpdateTextureFastDelayed), fastUpdateDelay);
        }

        public void UpdateTextureFastDelayed()
        {
            texture.Apply();
            fastUpdating = false;
        }

        public void UpdateTextureSlow()
        {
            if (slowUpdating)
                return;
            slowUpdating = true;
            SendCustomEventDelayedSeconds(nameof(UpdateTextureSlowDelayed), slowUpdateDelay);
        }

        public void UpdateTextureSlowDelayed()
        {
            if ((Networking.LocalPlayer.GetPosition() - this.transform.position).magnitude > 64f)
                UpdateTextureSuperSlow();
            else
                texture.Apply();
            slowUpdating = false;
        }

        public void UpdateTextureSuperSlow()
        {
            if (superSlowUpdating)
                return;
            superSlowUpdating = true;
            SendCustomEventDelayedSeconds(nameof(UpdateTextureSuperSlowDelayed), superSlowUpdateDelay);
        }

        public void UpdateTextureSuperSlowDelayed()
        {
            texture.Apply();
            superSlowUpdating = false;
        }

        #if UNITY_EDITOR && !COMPILER_UDONSHARP
        [InitializeOnLoad]
        public static class OnBuildRegister
        {
            static OnBuildRegister() => JanSharp.OnBuildUtil.RegisterType<Chalkboard>();
        }
        bool IOnBuildCallback.OnBuild()
        {
            chalkboardManager = GameObject.Find("/ChalkboardManager")?.GetUdonSharpComponent<ChalkboardManager>();
            boardId = chalkboardManager?.GetBoardId(this) ?? -1;
            if (chalkboardManager == null)
                Debug.LogError("Chalkboard requires a GameObject that must be at the root of the scene"
                        + " with the exact name 'ChalkboardManager' which has the 'ChalkboardManager' UdonBehaviour.",
                    UdonSharpEditorUtility.GetBackingUdonBehaviour(this));

            if (bottomLeft != null && topRight != null && material != null)
            {
                var blPos = bottomLeft.position;
                var trPos = topRight.position;

                var vertical = bottomLeft.up;
                var horizontal = bottomLeft.right;

                // blPos.x + X * horizontal.x + Y * vertical.x = trPos.x
                // blPos.y + X * horizontal.y + Y * vertical.y = trPos.y
                // blPos.z + X * horizontal.z + Y * vertical.z = trPos.z

                // blPos.x + X * horizontal.x + Y * vertical.x - trPos.x = 0
                // blPos.y + X * horizontal.y + Y * vertical.y - trPos.y = 0
                // blPos.z + X * horizontal.z + Y * vertical.z - trPos.z = 0

                boardParent = bottomLeft.parent;
                if (topRight.parent != boardParent)
                    Debug.LogError($"{nameof(bottomLeft)} and {nameof(topRight)} must share the same parent",
                        UdonSharpEditorUtility.GetBackingUdonBehaviour(this));

                var texture = (Texture2D)material.mainTexture;
                var pixelsPerUnit = new Vector3(
                    ((topRight.localPosition.x - bottomLeft.localPosition.x) / texture.width),
                    ((topRight.localPosition.y - bottomLeft.localPosition.y) / texture.height)
                );
                var lossyScale = boardParent.lossyScale;

                chalkScale = pixelsPerUnit * 5.75f;
                chalkScale = new Vector3(lossyScale.x * chalkScale.x, lossyScale.y * chalkScale.y, 0.01f);
                spongeScale = pixelsPerUnit * 41f;
                spongeScale = new Vector3(lossyScale.x * spongeScale.x, lossyScale.y * spongeScale.y, 0.01f);
            }

            if (bottomLeft == null || topRight == null || material == null)
                Debug.LogError($"{nameof(bottomLeft)}, {nameof(topRight)} and {nameof(material)} must all be set.",
                    UdonSharpEditorUtility.GetBackingUdonBehaviour(this));

            this.ApplyProxyModifications();
            // EditorUtility.SetDirty(UdonSharpEditorUtility.GetBackingUdonBehaviour(this));
            return chalkboardManager != null;
        }
        #endif

        public void Clear()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ClearInternal));
        }

        public void ClearInternal()
        {
            texture.SetPixels(initialPixels);
            allActions = new ulong[64]; // to free up that memory
            allActionsCount = 0;
            currentActions = 0;
            currentActionsIndex = 0;
            prevChalk = null;
            prevX = 0;
            prevY = 0;
            waitingToStartSending = false;
            sending = false;
            firstSend = false;
            catchingUp = false;
            catchingUpWithTheQueue = false;
            if (progressBar != null)
                progressBar.gameObject.SetActive(false);
            somebodyIsCatchingUp = false;
            receivedChalk = null; // just to clear the reference, really. syncing doesn't happen anymore
            catchUpQueue = null;
            catchUpQueueCount = 0; // reset to stop CatchUpWithQueue if it's running
            catchUpQueueIndex = 0; // reset to stop CatchUpWithQueue if it's running
            UpdateTextureSlow();
        }



        public void DrawPoint(Chalk chalk, int x, int y)
        {
            IncrementLocalUse();
            UseChalk(chalk);
            AddAction(x | (y << AxisBitCount));
            if (!catchingUp && !catchingUpWithTheQueue)
                DrawPointInternal(chalk, x, y);
            prevX = x;
            prevY = y;
        }

        public void DrawLine(Chalk chalk, int fromX, int fromY, int toX, int toY)
        {
            IncrementLocalUse();
            UseChalk(chalk);
            if (fromX != prevX || fromY != prevY)
                AddAction(fromX | (fromY << AxisBitCount));
            AddAction(toX | (toY << AxisBitCount) | IntPointHasPrev);
            if (!catchingUp && !catchingUpWithTheQueue)
                DrawLineInternal(chalk, fromX, fromY, toX, toY);
            prevX = toX;
            prevY = toY;
        }

        private void IncrementLocalUse()
        {
            // if some player is drawing on a board a lot then we'll try making them the owner of it.
            // that's purely done to spread out ownerships and with it load between players
            if (((++localUsageCount) % AttemptToTakeOwnershipInterval) == 0)
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }

        private void UseChalk(Chalk chalk)
        {
            if (chalk == prevChalk)
                return;
            prevChalk = chalk;
            // y == 1 is an invalid point, so it means "switch to chalk [x]" instead
            AddAction(chalk.chalkId | (IntSwitchToChalkY << AxisBitCount));
            prevX = 0;
            prevY = 0;
        }

        private void AddAction(int action)
        {
            currentActions |= ((ulong)action) << (currentActionsIndex * ActionBitCount);
            if (++currentActionsIndex == 3)
            {
                AddToAllActions(currentActions);
                currentActions = 0;
                currentActionsIndex = 0;
            }
        }

        private void AddToAllActions(ulong actions)
        {
            if (catchingUp || catchingUpWithTheQueue)
            {
                if (catchUpQueueCount == catchUpQueue.Length)
                {
                    var newCatchUpQueue = new ulong[catchUpQueueCount * 2];
                    catchUpQueue.CopyTo(newCatchUpQueue, 0);
                    catchUpQueue = newCatchUpQueue;
                }
                catchUpQueue[catchUpQueueCount++] = actions;
            }
            else
            {
                AddToAllActionsInternal(actions);
            }
        }

        private void AddToAllActionsInternal(ulong actions)
        {
            if (allActionsCount == allActions.Length)
            {
                var newAllActions = new ulong[allActionsCount * 2];
                allActions.CopyTo(newAllActions, 0);
                allActions = newAllActions;
            }
            allActions[allActionsCount++] = actions;
        }

        private void DrawPointInternal(Chalk chalk, int x, int y)
        {
            if (chalk.isSponge)
            {
                texture.SetPixels(x - chalk.halfSize, y - chalk.halfSize, chalk.size, chalk.size, chalk.colors);
            }
            else
            {
                int blX = x - chalk.halfSize;
                int blY = y - chalk.halfSize;
                int trX = x + chalk.halfSize;
                int trY = y + chalk.halfSize;
                chalk.colors[0] = texture.GetPixel(blX, blY);
                chalk.colors[4] = texture.GetPixel(trX, blY);
                chalk.colors[20] = texture.GetPixel(blX, trY);
                chalk.colors[24] = texture.GetPixel(trX, trY);
                texture.SetPixels(blX, blY, chalk.size, chalk.size, chalk.colors);
            }
        }

        private void DrawLineInternal(Chalk chalk, int fromX, int fromY, int toX, int toY)
        {
            Vector2 delta = new Vector2(toX - fromX, toY - fromY);
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)) // horizontal
            {
                int stepX = System.Math.Sign(delta.x) * chalk.lineDrawingFrequency;
                float stepY = (delta.y / Mathf.Abs(delta.x)) * chalk.lineDrawingFrequency;
                float y = fromY;
                if (fromX < toX)
                    for (int x = fromX + stepX; x <= toX - 1; x += stepX)
                        DrawPointInternal(chalk, x, Mathf.RoundToInt(y += stepY));
                else
                    for (int x = fromX + stepX; x >= toX + 1; x += stepX)
                        DrawPointInternal(chalk, x, Mathf.RoundToInt(y += stepY));
            }
            else // vertical
            {
                int stepY = System.Math.Sign(delta.y) * chalk.lineDrawingFrequency;
                float stepX = (delta.x / Mathf.Abs(delta.y)) * chalk.lineDrawingFrequency;
                float x = fromX;
                if (fromY < toY)
                    for (int y = fromY + stepY; y <= toY - 1; y += stepY)
                        DrawPointInternal(chalk, Mathf.RoundToInt(x += stepX), y);
                else
                    for (int y = fromY + stepY; y >= toY + 1; y += stepY)
                        DrawPointInternal(chalk, Mathf.RoundToInt(x += stepX), y);
            }
            DrawPointInternal(chalk, toX, toY);
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isMaster) // first player joining, we have to ignore it because they can't send to themselves so they would get stuck
                return;
            if (Networking.IsOwner(this.gameObject))
                InitSending();
            else if (player.isLocal) // technically this doesn't have to be an else if (I don't think) but it makes no sense to ever do both
            {
                catchingUp = true;
                if (progressBar != null)
                    progressBar.gameObject.SetActive(true);
                catchUpQueue = new ulong[8];
            }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            // I remember hearing something about "the master of the instance is always the one who's been the longest in the instance"
            // and I've also heard that "the default owner of any object is the master", which would make me guess that if the current
            // owner of this object leaves (or crashes, same thing) the player who's been in the instance the longest becomes the owner.
            // If that is true then that works in our favor because the player who's been in the instance the longest naturally has
            // the most information about all the boards in the world.
            // Even if this isn't the case, trying to figure out who still has the most information about a board and transferring
            // ownership to them is damn near impossible. Like it's not impossible but not reasonable or feasible
            //
            // also note that if the player who was sending for late joiners leaves/crashes then the new owner is just going to start
            // from the beginning. I thought about syncing the index for about where we're at, but it actually adds quite a lot of
            // complexity. So considering this isn't exactly common I'll call it good enough since it doesn't break at least... in theory
            if (somebodyIsCatchingUp && player.isLocal)
            {
                catchingUp = false;
                catchingUpWithTheQueue = false;
                if (progressBar != null)
                    progressBar.gameObject.SetActive(false);
                catchUpQueue = null;
                catchUpQueueCount = 0;
                catchUpQueueIndex = 0;
                receivedChalk = null;
                InitSending();
            }
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            return !somebodyIsCatchingUp && !catchingUp && !catchingUpWithTheQueue && !sending && !waitingToStartSending;
        }

        private void InitSending()
        {
            SendCustomEventDelayedSeconds(nameof(RequestSerializationDelayed), LateJoinerSyncDelay); // honestly... I'm annoyed
            somebodyIsCatchingUp = false;
            sending = false;
            waitingToStartSending = true;
        }

        public override void OnPreSerialization()
        {
            syncedActions1 = GetNextActions();
            syncedActions2 = GetNextActions();
            syncedActions3 = GetNextActions();
            syncedActions4 = GetNextActions();
            if (sending)
                SendCustomEventDelayedSeconds(nameof(RequestSerializationDelayed), SyncFrequency);
        }

        private ulong GetNextActions()
        {
            if (!sending)
            {
                Debug.Log($"<dlt> ICU VRC trying to screw me, no I'm not syncing data right now. Ok at this point the logic actually uses this intentionally.");
                return 0UL;
            }
            if (firstSend)
            {
                // for some reason `|` doesn't understand the difference between implicit and explicit casts
                // so it's still complaining even with an explicit cast. Just using `+` instead because
                // none of the bits will be used twice anyway so it does the same thing
                Debug.Log($"<dlt> informing everyone that we're about to sync {actionsCountRequiredToSync} actions");
                firstSend = false;
                return MetadataFlag | ActionCountMetadataFlag + (ulong)actionsCountRequiredToSync;
            }
            Debug.Log($"<dlt> sending {currentSyncedIndex + 1}/{actionsCountRequiredToSync + 1}");
            if (currentSyncedIndex >= actionsCountRequiredToSync)
            {
                sending = false;
                return MetadataFlag;
            }
            return allActions[currentSyncedIndex++];
        }

        public void RequestSerializationDelayed()
        {
            if (waitingToStartSending)
            {
                // it is at this point that the joined player can actually receive the packets we are sending
                // so initialize how far we need to sync here instead of in on player joined
                AddToAllActions(currentActions);
                currentActions = 0;
                currentActionsIndex = 0;
                actionsCountRequiredToSync = allActionsCount;
                currentSyncedIndex = 0;
                waitingToStartSending = false;
                firstSend = true;
                sending = true;
            }
            RequestSerialization();
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            Debug.Log($"<dlt> on post: success: {result.success}, byteCount: {result.byteCount}");
        }

        public override void OnDeserialization()
        {
            ProcessSyncedActions(syncedActions1);
            ProcessSyncedActions(syncedActions2);
            ProcessSyncedActions(syncedActions3);
            ProcessSyncedActions(syncedActions4);
        }

        private void ProcessSyncedActions(ulong syncedActions)
        {
            if ((syncedActions & MetadataFlag) != 0UL)
            {
                ulong metadata = syncedActions ^ MetadataFlag; // remove metadata flag
                if ((metadata & ActionCountMetadataFlag) != 0UL)
                {
                    Debug.Log($"<dlt> someone (could be multiple people) is about to receive {actionsCountRequiredToSync} actions");
                    metadata ^= ActionCountMetadataFlag; // remove second flag
                    if (catchingUp)
                    {
                        currentReceivedActionIndex = 0;
                        expectedReceivedActionsCount = (int)metadata;
                    }
                    else
                        somebodyIsCatchingUp = true;
                    return;
                }
                if (catchingUp)
                {
                    Debug.Log($"<dlt> we caught up with all actions that happened before we joined!");
                    catchingUp = false;
                    catchingUpWithTheQueue = true;
                    SendCustomEventDelayedFrames(nameof(CatchUpWithQueue), 1);
                    return;
                }
                somebodyIsCatchingUp = false;
                return;
            }
            if (!catchingUp)
                return;
            ProcessActions(syncedActions);
            if (progressBar != null)
                progressBar.value = (float)(++currentReceivedActionIndex) / (float)expectedReceivedActionsCount;
        }

        private void ProcessActions(ulong actions)
        {
            bool doUpdateTexture = false;
            for (int i = 0; i < 3; i++)
            {
                int point = (int)((actions >> (i * ActionBitCount)) & ActionBits);
                if (point == IntUnusedAction)
                    break;
                doUpdateTexture = true;
                int x = point & IntAxisBits;
                int y = (point >> AxisBitCount) & IntAxisBits;
                if (y == IntSwitchToChalkY)
                {
                    Debug.Log($"<dlt> processing switch to chalk id: {x}");
                    receivedChalk = chalkboardManager.chalks[x];
                }
                else
                {
                    Debug.Log($"<dlt> processing point x: {x}, y: {y} hasPrev: {((point & IntPointHasPrev) != 0)}");
                    if (receivedChalk == null)
                        Debug.Log($"<dlt> processing point before receiving any switch to a chalk?!");
                    else if ((point & IntPointHasPrev) != 0)
                        DrawLineInternal(receivedChalk, receivedPrevX, receivedPrevY, x, y);
                    else
                        DrawPointInternal(receivedChalk, x, y);
                    receivedPrevX = x;
                    receivedPrevY = y;
                }
            }
            AddToAllActionsInternal(actions);
            if (doUpdateTexture)
                UpdateTextureSlow();
        }

        public void CatchUpWithQueue()
        {
            Debug.Log($"<dlt> CatchUpWithQueue {catchUpQueueIndex + 1}/{catchUpQueueCount + 1}");
            if (catchUpQueueIndex == catchUpQueueCount)
            {
                Debug.Log($"<dlt> we are fully caught up!");
                catchingUpWithTheQueue = false;
                catchUpQueue = null; // free that memory
                if (progressBar != null)
                    progressBar.gameObject.SetActive(false);
                return;
            }
            ProcessActions(catchUpQueue[catchUpQueueIndex++]);
            if (progressBar != null)
                progressBar.value = (float)catchUpQueueIndex / (float)catchUpQueueCount;
            SendCustomEventDelayedFrames(nameof(CatchUpWithQueue), 1);
        }
    }
}
