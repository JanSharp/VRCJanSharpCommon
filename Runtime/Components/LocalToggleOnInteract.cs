using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalToggleOnInteract : UdonSharpBehaviour
    {
        public GameObject toToggle;
        public string activateText;
        public string deactivateText;

        public override void Interact()
        {
            bool activeSelf = !toToggle.activeSelf;
            toToggle.SetActive(activeSelf);
            InteractionText = activeSelf ? deactivateText : activateText;
        }
    }
}
