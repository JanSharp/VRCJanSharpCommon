using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class UIToggleSync : UdonSharpBehaviour
    {
        [Tooltip("Must not be null.")]
        [SerializeField] private Toggle toggle;
        [Tooltip("Should this script change the state of the toggle with or without notifying OnValueChanged "
            + "listeners of the Toggle?\nIf all scripts listening to said event are themselves also synced "
            + "then this can be disabled to prevent odd network race conditions.")]
        [PublicAPI] public bool doNotifyOnReceive = true;

        [UdonSynced][SerializeField][HideInInspector] private bool isOn; // Set in OnBuild.

        private const float MinBackOffTime = 1f;
        private const float MaxBackOffTime = 16f;
        private float currentBackOffTime = MinBackOffTime;

        public override void OnDeserialization()
        {
#if JAN_SHARP_COMMON_DEBUG
            Debug.Log($"[JanSharpCommonDebug] UIToggleSync  OnDeserialization - isOn: {isOn}");
#endif
            if (doNotifyOnReceive)
                toggle.isOn = isOn;
            else
                toggle.SetIsOnWithoutNotify(isOn);
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
            Debug.Log($"[JanSharpCommonDebug] UIToggleSync  OnValueChanged - toggle.isOn: {toggle.isOn}");
#endif
            bool toggleIsOn = toggle.isOn;
            if (toggleIsOn == isOn)
                return;

            isOn = toggleIsOn;
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            RequestSerialization();
        }
    }
}
