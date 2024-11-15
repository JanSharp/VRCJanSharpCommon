using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [DefaultExecutionOrder(-100000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonScript]
    public class WannaBeClassesManager : UdonSharpBehaviour
    {
        [SerializeField] private string[] wannaBeClassNames;
        [SerializeField] private GameObject[] wannaBeClassPrefabs;
        [SerializeField] private Transform[] instancesExistingAtBuildTime;

        private DataDictionary prefabsLut;
        private DataDictionary PrefabsLut
        {
            get
            {
                if (prefabsLut != null)
                    return prefabsLut;
                prefabsLut = new DataDictionary();
                int c = wannaBeClassNames.Length;
                for (int i = 0; i < c; i++)
                    prefabsLut.Add(wannaBeClassNames[i], wannaBeClassPrefabs[i]);
                return prefabsLut;
            }
        }

        private void Start()
        {
            foreach (Transform instance in instancesExistingAtBuildTime)
                if (instance != null)
                {
                    instance.SetParent(this.transform);
                    instance.localPosition = Vector3.zero;
                    instance.localRotation = Quaternion.identity;
                    instance.localScale = Vector3.one;
                }
            // Don't hold on to those references so they can be garbage collected at some point.
            instancesExistingAtBuildTime = null;
        }

        public bool TryGetPrefabInternal(string wannaBeClassName, out GameObject prefab)
        {
            prefab = null;
            if (!PrefabsLut.TryGetValue(wannaBeClassName, out DataToken prefabToken))
            {
                Debug.LogError($"[JanSharpCommon] Attempt to construct an instance of the WannaBeClass {wannaBeClassName}, however no such class exists.");
                return false;
            }
            prefab = (GameObject)prefabToken.Reference;
            return true;
        }
    }

    public static class WannaBeClassesManagerExtensions
    {
        public static T New<T>(this WannaBeClassesManager manager, string wannaBeClassName)
            where T : WannaBeClass
        {
            // Can't use typeof(T).Name, unfortunately.
            return (T)NewInternal(manager, wannaBeClassName);
            // Having NewInternal be a separate function without a generic type parameter enables UdonSharp to
            // only generate 1 instance of the NewInternal function for each script that needs it, rather than
            // generating one for each unique generic type parameter. So it's just deduplication resulting in
            // ever so slightly smaller generated scripts.
        }

        private static WannaBeClass NewInternal(WannaBeClassesManager manager, string wannaBeClassName)
        {
            if (!manager.TryGetPrefabInternal(wannaBeClassName, out GameObject prefab))
                return null;
            // By having this logic in a static function it gets put into each script which calls the New,
            // function, which then means that multiple scripts can call New inside of their
            // WannaBeConstructor without running into recursion issues.
            GameObject go = Object.Instantiate(prefab, manager.transform);
            WannaBeClass inst = (WannaBeClass)go.GetComponent<UdonSharpBehaviour>();
            // inst.SetProgramVariable("wannaBeClasses", manager); // Not needed because the "prefab" already has it set.
            inst.WannaBeConstructor();
            return inst;
        }
    }
}
