using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

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

        private DataList[] pooledClassInstances;

        private bool initialized = false;

        /// <summary>
        /// <para>For use by anything that is potentially recursive, and supports recursion.</para>
        /// </summary>
        private WannaBeClass[] classStack = new WannaBeClass[ArrList.MinCapacity];
        private int classStackCount = 0;

        private void Initialize()
        {
            if (initialized)
                return;
            initialized = true;
            int count = wannaBeClassNames.Length;
            pooledClassInstances = new DataList[count];
        }

        private void Start()
        {
            Initialize();
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

        private WannaBeClass GetPooledClassInstance(int classIndex)
        {
            if (!initialized)
                return null;

            DataList pooled = pooledClassInstances[classIndex];
            if (pooled == null)
                return null;
            int count = pooled.Count;
            if (count == 0)
                return null;

            WannaBeClass result = (WannaBeClass)pooled[--count].Reference;
            pooled.RemoveAt(count);
            return result;
        }

        private WannaBeClass CreateClassInstance(int classIndex)
        {
            GameObject prefab = wannaBeClassPrefabs[classIndex];
#if JAN_SHARP_COMMON_STOPWATCH
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            GameObject go = Instantiate(prefab, instancesParent);
#if JAN_SHARP_COMMON_STOPWATCH
            double instantiateMs = sw.Elapsed.TotalMilliseconds;
            Debug.Log($"[JanSharpCommonDebug] [sw] WannaBeClassesManager  NewDynamic (inner) - instantiateMs: {instantiateMs:f3}, wannaBeClassName: {wannaBeClassNames[classIndex]}");
#endif
            // No need to set the wannaBeClasses variable, the "prefab" already has it set.
            return (WannaBeClass)go.GetComponent<UdonSharpBehaviour>();
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

            WannaBeClass inst = GetPooledClassInstance(classIndex);
            if (inst != null)
            {
                inst.SetProgramVariable("referencesCount", 1);
                inst.SetProgramVariable("hasBeenDestructed", false);
            }
            else
                inst = CreateClassInstance(classIndex);

            ArrList.Add(ref classStack, ref classStackCount, inst);
            inst.WannaBeConstructor(); // Similarly, this could call NewDynamic, or in other words: be recursive.
            return classStack[--classStackCount];
        }

        public void Delete(WannaBeClass instance)
        {
            bool hasBeenDestructed = (bool)instance.GetProgramVariable("hasBeenDestructed");
            if (hasBeenDestructed)
                return;
            instance.SetProgramVariable("hasBeenDestructed", true);
            ArrList.Add(ref classStack, ref classStackCount, instance);
            instance.WannaBeDestructor(); // This could call Delete, or in other words: be recursive.
            instance = classStack[--classStackCount];
            if (!instance.WannaBeClassSupportsPooling)
            {
                Destroy(instance.gameObject);
                return;
            }

            if (!initialized)
                Initialize();
            int classIndex = (int)instance.GetProgramVariable("internalClassIndex");
            DataList pooled = pooledClassInstances[classIndex];
            if (pooled == null)
            {
                pooled = new DataList();
                pooledClassInstances[classIndex] = pooled;
            }
            pooled.Add(instance);
            instance.ResetWannaBeClassToDefault();
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
