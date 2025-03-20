using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalEventOnInteract : UdonSharpBehaviour
    {
        public UdonBehaviour target;
        public string eventName;
        public override void Interact() => target.SendCustomEvent(eventName);
    }
}
