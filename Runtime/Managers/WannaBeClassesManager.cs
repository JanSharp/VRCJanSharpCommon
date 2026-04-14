using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [DefaultExecutionOrder(-100000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonScript("4fde516c38225afdf98eec7b044ed853")] // Runtime/Prefabs/WannaBeClassesManager.prefab
    public class WannaBeClassesManager : UdonSharpBehaviour
    {
        [SerializeField] private Transform prefabsParent;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public Transform PrefabsParent => prefabsParent;
#endif
        [SerializeField] private Transform instancesParent;
        [SerializeField][HideInInspector] private string[] wannaBeClassNames;
        [SerializeField][HideInInspector] private GameObject[] wannaBeClassPrefabs;
        [SerializeField][HideInInspector] private WannaBeClass[] instancesExistingAtBuildTime;

        /// <summary>
        /// <para>For use by anything that is potentially recursive, and supports recursion.</para>
        /// </summary>
        private WannaBeClass[] classStack = new WannaBeClass[ArrList.MinCapacity];
        private int classStackCount = 0;

        private void Start()
        {
            foreach (WannaBeClass instance in instancesExistingAtBuildTime)
                if (instance != null)
                {
                    Transform t = instance.transform;
                    t.SetParent(instancesParent);
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                    t.localScale = Vector3.one;
                    instance.WannaBeConstructor();
                }
            // Don't hold on to those references so they can be garbage collected at some point.
            instancesExistingAtBuildTime = null;
        }

        public WannaBeClass NewDynamic(string wannaBeClassName)
        {
            int classIndex = System.Array.BinarySearch(wannaBeClassNames, wannaBeClassName);
            if (classIndex < 0)
            {
                Debug.LogError($"[JanSharpCommon] Attempt to construct an instance of a WannaBeClass by "
                    + $"the name '{wannaBeClassName}', however no such class exists.");
                return null;
            }
            GameObject prefab = wannaBeClassPrefabs[classIndex];

#if JAN_SHARP_COMMON_STOPWATCH
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            GameObject go = Instantiate(prefab, instancesParent);
#if JAN_SHARP_COMMON_STOPWATCH
            double instantiateMs = sw.Elapsed.TotalMilliseconds;
            Debug.Log($"[JanSharpCommonDebug] [sw] WannaBeClassesManager  NewDynamic (inner) - instantiateMs: {instantiateMs:f3}, wannaBeClassName: {wannaBeClassName}");
#endif
            WannaBeClass inst = (WannaBeClass)go.GetComponent<UdonSharpBehaviour>();
            // inst.SetProgramVariable("wannaBeClasses", manager); // Not needed because the "prefab" already has it set.
            ArrList.Add(ref classStack, ref classStackCount, inst);
            inst.WannaBeConstructor(); // This could call NewDynamic, or in other words: be recursive.
            return classStack[--classStackCount];
        }
    }

    public static class WannaBeClassesManagerExtensions
    {
        public static T New<T>(this WannaBeClassesManager manager, string wannaBeClassName)
            where T : WannaBeClass
        {
            // Can't use typeof(T).Name, unfortunately.
            return (T)manager.NewDynamic(wannaBeClassName);
        }
    }
}
