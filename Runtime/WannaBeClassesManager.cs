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

        public WannaBeClass NewInternal(string wannaBeClassName)
        {
            if (!PrefabsLut.TryGetValue(wannaBeClassName, out DataToken prefabToken))
            {
                Debug.LogError($"[JanSharpCommon] Attempt to construct an instance of the WannaBeClass {wannaBeClassName}, however no such class exists.");
                return null;
            }
            GameObject go = Instantiate((GameObject)prefabToken.Reference, this.transform);
            WannaBeClass inst = (WannaBeClass)go.GetComponent<UdonSharpBehaviour>();
            inst.SetProgramVariable("wannaBeClasses", this);
            inst.WannaBeConstructor();
            return inst;
        }

        public void Delete(WannaBeClass wannaBeClassInstance)
        {
            wannaBeClassInstance.WannaBeDestructor();
            Destroy(wannaBeClassInstance.gameObject);
        }
    }

    public static class WannaBeClassesManagerExtensions
    {
        public static T New<T>(this WannaBeClassesManager manager, string wannaBeClassName)
            where T : WannaBeClass
        {
            // Can't use typeof(T).Name, unfortunately.
            return (T)manager.NewInternal(wannaBeClassName);
        }
    }
}
