using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalToggleMultipleOnInteract : UdonSharpBehaviour
    {
        [Tooltip("Upon interaction, all theses GameObjects get toggled - they flip their active state. "
            + "This means that these GameObjects can be a mixture of active and inactive ones, they all "
            + "simply get their active state inverted.")]
        public GameObject[] toToggle;
        [SerializeField] private bool changeInteractionText = false;
        [SerializeField] private string activateText = "Use";
        [SerializeField] private string deactivateText = "Use";

        private bool state = false;

        public override void Interact()
        {
            foreach (GameObject go in toToggle)
                if (go != null)
                    go.SetActive(!go.activeSelf);
            state = !state;
            if (changeInteractionText)
                InteractionText = state ? deactivateText : activateText;
        }
    }
}
