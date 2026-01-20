using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonDependency(typeof(ShowObjectsByPlatformManager))]
    public class ShowObjectByPlatform : UdonSharpBehaviour
    {
        [SerializeField] private bool showInVR = false;
        [SerializeField] private bool showInDesktop = false;
        private bool resolved = false;

        private void Start() => Resolve(); // To support instantiation.

        public void Resolve()
        {
            if (resolved)
                return;
            resolved = true;
            gameObject.SetActive(Networking.LocalPlayer.IsUserInVR() ? showInVR : showInDesktop);
        }
    }
}
