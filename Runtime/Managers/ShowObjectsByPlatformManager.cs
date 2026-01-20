using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonScript("3da0833b941ab24eabdfbcb8236bcd04")] // Runtime/Prefabs/ShowObjectByPlatformManager.prefab
    public class ShowObjectsByPlatformManager : UdonSharpBehaviour
    {
        [HideInInspector][SerializeField] private ShowObjectByPlatform[] showObjectScripts;
        [HideInInspector][SerializeField] private ShowObjectsByPlatform[] showObjectsScripts;

        private void Start()
        {
            // This manager exists to prevent flashing of objects the first time they get enabled
            // if they are disabled in hierarchy by default.
            foreach (ShowObjectByPlatform script in showObjectScripts)
                if (script != null)
                    script.Resolve();
            foreach (ShowObjectsByPlatform script in showObjectsScripts)
                if (script != null)
                    script.Resolve();
        }
    }
}
