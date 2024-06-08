using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using JetBrains.Annotations;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIToggleSendLocalEvent : UdonSharpBehaviour
    {
        [Tooltip("Must not be null.")]
        [SerializeField] private Toggle toggle;
        [SerializeField] [HideInInspector] private bool wasOn; // Set in OnBuild.

        [Space]

        [Tooltip("Can be null.")]
        /// <summary>Can be null.</summary>
        [PublicAPI] public UdonBehaviour onTurnOnTarget;
        [Tooltip("If On Turn On Target is not null, this should not be empty.")]
        /// <summary>If onTurnOnTarget is not null, this should not be empty.</summary>
        [PublicAPI] public string onTurnOnEventName;
        [Space]
        [Tooltip("Can be null.")]
        /// <summary>Can be null.</summary>
        [PublicAPI] public UdonBehaviour onTurnOffTarget;
        [Tooltip("If On Turn Off Target is not null, this should not be empty.")]
        /// <summary>If onTurnOffTarget is not null, this should not be empty.</summary>
        [PublicAPI] public string onTurnOffEventName;
        [Space]
        [Tooltip("Can be null.")]
        /// <summary>Can be null.</summary>
        [PublicAPI] public UdonBehaviour onValueChangedTarget;
        [Tooltip("If On Value Changed Target is not null, this should not be empty.")]
        /// <summary>If onValueChangedTarget is not null, this should not be empty.</summary>
        [PublicAPI] public string onValueChangedEventName;

        /// <summary>
        /// Only passes along the event if the 'isOn' did actually change. If it didn't change, it's ignored.
        /// </summary>
        [PublicAPI] public void OnValueChanged()
        {
            bool isOn = toggle.isOn;
            if (isOn == wasOn)
                return;
            wasOn = isOn;

            if (isOn)
            {
                if (onTurnOnTarget != null)
                    onTurnOnTarget.SendCustomEvent(onTurnOnEventName);
            }
            else
            {
                if (onTurnOffTarget != null)
                    onTurnOffTarget.SendCustomEvent(onTurnOffEventName);
            }

            if (onValueChangedTarget != null)
                onValueChangedTarget.SendCustomEvent(onValueChangedEventName);
        }
    }
}
