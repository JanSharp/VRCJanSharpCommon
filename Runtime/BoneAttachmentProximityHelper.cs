using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp.Internal
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BoneAttachmentProximityHelper : UdonSharpBehaviour
    {
        [SerializeField] private BoneAttachmentManager manager;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player) => manager.OnPlayerGettingClose(player);
    }
}
