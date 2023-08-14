using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InteractProxy : UdonSharpBehaviour
    {
        [Tooltip("Target behaviour to pass the Interact event to")]
        [SerializeField] private UdonBehaviour target;

        public override void Interact()
        {
            // This is 100% a hack, but all things considered it's the best option.
            // Especially since the hack actually has a decently high chance of not getting broken by updates.
            // Oh and why is it a hack? It's sending an event to the internal name of the Interact entry point.
            target.SendCustomEvent("_interact");
        }
    }
}
