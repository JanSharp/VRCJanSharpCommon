using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalEventOnLocalStationExited : UdonSharpBehaviour
    {
        public UdonBehaviour target;
        public string eventName;

        public override void OnStationExited(VRCPlayerApi player)
        {
            if (!player.isLocal)
                return;
            target.SendCustomEvent(eventName);
        }
    }
}
