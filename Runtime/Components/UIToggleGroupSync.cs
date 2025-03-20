using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using JetBrains.Annotations;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class UIToggleGroupSync : UdonSharpBehaviour
    {
        [SerializeField] private ToggleGroup toggleGroup;
        [Tooltip("Must be all the toggles which are referencing the given toggle group.")]
        [SerializeField] private bool automaticallyUseChildToggles;
        [SerializeField] private Toggle[] togglesInGroup;

        [UdonSynced] [HideInInspector] [SerializeField] private int activeToggleIndex = -1; // Set in OnBuild.

        private const float MinBackOffTime = 1f;
        private const float MaxBackOffTime = 16f;
        private float currentBackOffTime = MinBackOffTime;

        private void Start() // TODO: use editor scripting
        {
            activeToggleIndex = -1;
            for (int i = 0; i < togglesInGroup.Length; i++)
            {
                Toggle toggle = togglesInGroup[i];
                if (toggle == null)
                    continue;
                if (toggle.isOn)
                {
                    activeToggleIndex = i;
                    break;
                }
            }
        }

        public override void OnDeserialization()
        {
            if (activeToggleIndex == -1)
            {
                toggleGroup.SetAllTogglesOff();
                return;
            }
            Toggle toggle = togglesInGroup[activeToggleIndex];
            if (toggle == null)
            {
                Debug.LogError($"[JanSharp Common] The toggle at index {activeToggleIndex} is null locally "
                    + "when it should exist since deserialization says it should be toggled on.");
                return;
            }
            toggle.isOn = true;
            // toggleGroup.NotifyToggleOn(toggle); // Not exposed.
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            if (!result.success)
            {
                SendCustomEventDelayedSeconds(nameof(InternalRequestSerializationDelayed), currentBackOffTime);
                // Exponential back off.
                currentBackOffTime = Mathf.Min(currentBackOffTime * 2, MaxBackOffTime);
            }
            else
                currentBackOffTime = MinBackOffTime;
        }

        /// <summary>This is not public API, do not call this function.</summary>
        public void InternalRequestSerializationDelayed() => RequestSerialization();

        [PublicAPI] public void OnValueChanged()
        {
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
