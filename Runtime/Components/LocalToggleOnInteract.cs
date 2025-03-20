using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalToggleOnInteract : UdonSharpBehaviour
    {
        public UdonBehaviour self;
        public GameObject toToggle;
        public string activateText;
        public string deactivateText;

        public override void Interact()
        {
            bool activeSelf = !toToggle.activeSelf;
            toToggle.SetActive(activeSelf);
            self.InteractionText = activeSelf ? deactivateText : activateText;
        }
    }
}
