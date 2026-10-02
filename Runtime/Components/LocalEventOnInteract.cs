using UdonSharp;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalEventOnInteract : UdonSharpBehaviour
    {
        public UdonBehaviour target;
        public string eventName;
        // Let this error when target is null, there is no point in this script existing in a scene
        // when the target is null, so map creators should fix it.
        public override void Interact() => target.SendCustomEvent(eventName);
    }
}
