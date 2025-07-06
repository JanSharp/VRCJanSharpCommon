using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class UIToggleGroupSync : UdonSharpBehaviour
    {
        [Tooltip("Must not be null.")]
        [SerializeField] private ToggleGroup toggleGroup;
        [Tooltip("Should this script change the state of the toggles with or without notifying OnValueChanged "
            + "listeners of the Toggles?\nIf all scripts listening to said events are themselves also synced "
            + "then this can be disabled to prevent odd network race conditions.")]
        [PublicAPI] public bool doNotifyOnReceive = true;

        // Set in OnBuild.
        [HideInInspector][SerializeField] private Toggle[] togglesInGroup;
        [UdonSynced][HideInInspector][SerializeField] private int activeToggleIndex = -1;
        private bool isReceiving = false;

        private const float MinBackOffTime = 1f;
        private const float MaxBackOffTime = 16f;
        private float currentBackOffTime = MinBackOffTime;

        private void SetActiveToggle(Toggle toActivate)
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] UIToggleGroupSync  SetActiveToggle - toActivate: {(toActivate == null ? "<null>" : toActivate.name)}");
#endif
            isReceiving = true;
            bool allowSwitchOff = toggleGroup.allowSwitchOff;
            if (!allowSwitchOff)
                toggleGroup.allowSwitchOff = true;

            foreach (Toggle toggle in togglesInGroup)
            {
                if (toggle == null || toggle == toActivate)
                    continue;

                if (doNotifyOnReceive)
                    toggle.isOn = false;
                else
                    toggle.SetIsOnWithoutNotify(false);
            }

            if (toActivate != null)
            {
                if (doNotifyOnReceive)
                    toActivate.isOn = true;
                else
                    toActivate.SetIsOnWithoutNotify(true);
            }

            if (!allowSwitchOff)
                toggleGroup.allowSwitchOff = false;
            isReceiving = false;
        }

        public override void OnDeserialization()
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] UIToggleGroupSync  OnDeserialization - activeToggleIndex: {activeToggleIndex}");
#endif
            if (activeToggleIndex == -1)
            {
                SetActiveToggle(null);
                return;
            }

            Toggle toggle = togglesInGroup[activeToggleIndex];
            if (toggle == null)
            {
                Debug.LogError($"[JanSharpCommon] The toggle at index {activeToggleIndex} is null locally "
                    + "when it should exist since deserialization says it should be toggled on.");
                return;
            }

            SetActiveToggle(toggle);
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            if (result.success)
                currentBackOffTime = MinBackOffTime;
            else
            {
                SendCustomEventDelayedSeconds(nameof(RequestSerializationDelayed), currentBackOffTime);
                currentBackOffTime = Mathf.Min(currentBackOffTime * 2, MaxBackOffTime); // Exponential back off.
            }
        }

        /// <summary>This is not public API, do not call this function.</summary>
        public void RequestSerializationDelayed() => RequestSerialization();

        [PublicAPI]
        public void OnValueChanged()
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] UIToggleGroupSync  OnValueChanged - isReceiving: {isReceiving}");
#endif
            if (isReceiving)
                return;

            int newActiveToggleIndex = -1;
            for (int i = 0; i < togglesInGroup.Length; i++)
            {
                Toggle toggle = togglesInGroup[i];
                if (toggle == null)
                    continue;
                if (toggle.isOn)
                {
                    newActiveToggleIndex = i;
                    break;
                }
            }
            if (newActiveToggleIndex == activeToggleIndex)
                return;

            activeToggleIndex = newActiveToggleIndex;
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            RequestSerialization();
        }
    }
}
