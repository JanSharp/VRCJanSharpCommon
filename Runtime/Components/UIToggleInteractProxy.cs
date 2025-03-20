using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using JetBrains.Annotations;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIToggleInteractProxy : UdonSharpBehaviour
    {
        [Tooltip("Must not be null.")]
        [SerializeField] private Toggle toggle;
        [SerializeField] [HideInInspector] private bool wasOn; // Set in OnBuild.

        [Space]

        [Tooltip("Can be null.")]
        /// <summary>Can be null.</summary>
        [PublicAPI] public UdonBehaviour onTurnOn;
        [Tooltip("Can be null.")]
        /// <summary>Can be null.</summary>
        [PublicAPI] public UdonBehaviour onTurnOff;
        [Tooltip("Can be null.")]
        /// <summary>Can be null.</summary>
        [PublicAPI] public UdonBehaviour onValueChanged;

        /// <summary>
        /// Only passes along the event if the 'isOn' did actually change. If it didn't change, it's ignored.
        /// </summary>
        [PublicAPI] public void OnValueChanged()
        {
            // This is 100% a hack, but all things considered it's the best option.
            // Especially since the hack actually has a decently high chance of not getting broken by updates.
            // Oh and why is it a hack? It's sending an event to the internal name of the Interact entry point.

            bool isOn = toggle.isOn;
            if (isOn == wasOn)
                return;
            wasOn = isOn;

            if (isOn)
            {
                if (onTurnOn != null)
                    onTurnOn.SendCustomEvent("_interact");
            }
            else
            {
                if (onTurnOff != null)
                    onTurnOff.SendCustomEvent("_interact");
            }

            if (onValueChanged != null)
                onValueChanged.SendCustomEvent("_interact");
        }
    }
}
