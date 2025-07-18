using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class OnTrulyPostLateUpdateAttribute : CustomRaisedEventBaseAttribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        public OnTrulyPostLateUpdateAttribute()
            : base((int)Internal.OnTrulyPostLateUpdateDummy.OnTrulyPostLateUpdate)
        { }
    }
}

namespace JanSharp.Internal
{
    public enum OnTrulyPostLateUpdateDummy
    {
        OnTrulyPostLateUpdate = 1,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [CustomRaisedEventsDispatcher(typeof(OnTrulyPostLateUpdateAttribute), typeof(OnTrulyPostLateUpdateDummy))]
    [SingletonScript("6f92a81d0ddb82621bd46248c4144027")] // Runtime/Prefabs/TrulyPostLateUpdate.prefab
    public class TrulyPostLateUpdateManager : UdonSharpBehaviour
    {
        [HideInInspector][SerializeField] private UdonSharpBehaviour[] onTrulyPostLateUpdateListeners;

        private VRCPlayerApi localPlayer;
        private Transform parentTransform;
        private float prevEventTime = -1f;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            parentTransform = transform.parent;
        }

        public override void PostLateUpdate()
        {
            var head = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            parentTransform.SetPositionAndRotation(head.position, head.rotation);
        }

        private void OnWillRenderObject()
        {
            float time = Time.time;
            if (prevEventTime == time)
                return;
            prevEventTime = time;
            // For some reason UdonSharp needs the 'JanSharp.' namespace name here to resolve the Raise function call.
            JanSharp.CustomRaisedEvents.Raise(ref onTrulyPostLateUpdateListeners, nameof(OnTrulyPostLateUpdateDummy.OnTrulyPostLateUpdate));
        }
    }
}
