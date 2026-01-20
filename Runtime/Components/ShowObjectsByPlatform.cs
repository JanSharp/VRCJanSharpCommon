using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonDependency(typeof(ShowObjectsByPlatformManager))]
    public class ShowObjectsByPlatform : UdonSharpBehaviour
    {
        [SerializeField] private bool showInVR = false;
        [SerializeField] private bool showInDesktop = false;
        [SerializeField] private GameObject[] gameObjects;
        private bool resolved = false;

        private void Start() => Resolve(); // To support instantiation.

        public void Resolve()
        {
            if (resolved)
                return;
            resolved = true;
            bool show = Networking.LocalPlayer.IsUserInVR() ? showInVR : showInDesktop;
            foreach (GameObject obj in gameObjects)
                if (obj != null)
                    obj.SetActive(show);
        }
    }
}
