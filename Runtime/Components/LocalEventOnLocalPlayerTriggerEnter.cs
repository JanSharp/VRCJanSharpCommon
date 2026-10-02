using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalEventOnLocalPlayerTriggerEnter : UdonSharpBehaviour
    {
        public UdonBehaviour target;
        public string eventName;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            // Let this error when target is null, there is no point in this script existing in a scene
            // when the target is null, so map creators should fix it.
            if (player.isLocal)
                target.SendCustomEvent(eventName);
        }
    }
}
