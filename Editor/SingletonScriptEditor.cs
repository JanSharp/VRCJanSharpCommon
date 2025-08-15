using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace JanSharp.Internal
{
    public interface ISingletonDependencyResolver
    {
        /// <summary>
        /// <para>Leave <c>fieldName</c> <see langword="null"/> in order to have a dependency without the
        /// given type having a field that the referenced singleton needs to get assigned to.</para>
        /// <para>Note that for some implementations of this function it is convenient to use
        /// <see langword="yield"/> <see langword="return"/>.</para>
        /// </summary>
        public IEnumerable<(string fieldName, System.Type singletonType, bool optional)> Resolve(System.Type ubType);
    }

    [InitializeOnLoad]
    [DefaultExecutionOrder(-990)]
    public static class SingletonScriptEditor
    {
        private class SingletonData
        {
            public System.Type singletonType;
            public UdonSharpBehaviour inst;
            public string prefabGuid;

            public SingletonData(System.Type singletonType, UdonSharpBehaviour inst, string prefabGuid)
            {
                this.singletonType = singletonType;
                this.inst = inst;
                this.prefabGuid = prefabGuid;
            }
        }

        private static SingletonManager singletonManager;
        private static Dictionary<System.Type, SingletonData> singletons = new();
        private static Dictionary<string, List<SingletonData>> singletonsByAssetGuid = new();
        private static List<SingletonData> singletonsList = new();
        /// <summary>
        /// <para>Not just fields, also includes dependencies, which simply have null field names.</para>
        /// </summary>
        private static Dictionary<System.Type, List<(string fieldName, System.Type singletonType, bool optional)>> typeCache = new();
        private static List<ISingletonDependencyResolver> customDependencyResolvers = new();
        private const BindingFlags PrivateAndPublicFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        static SingletonScriptEditor()
        {
            singletonManager = null; // Just for cleanliness. The rest are very much needed.
            singletons.Clear();
            singletonsByAssetGuid.Clear();
            singletonsList.Clear();
            typeCache.Clear();
            customDependencyResolvers.Clear();

            // Must always register this in order for the EnsureSingletonExists api to update the singleton manager
            OnBuildUtil.RegisterType<SingletonManager>(OnSingletonManagerBuild, order: -149);

            IEnumerable<System.Type> ubTypes = OnAssemblyLoadUtil.AllUdonSharpBehaviourTypes
                .Where(t => t.IsDefined(typeof(SingletonScriptAttribute), inherit: false));
            if (!ubTypes.Any())
                return;
            singletonsList = ubTypes
                .Select(t => new SingletonData(t, null, t.GetCustomAttribute<SingletonScriptAttribute>().PrefabGuid))
                .ToList();
            singletons = singletonsList.ToDictionary(s => s.singletonType, s => s);
            singletonsByAssetGuid = singletonsList
                .GroupBy(s => s.prefabGuid)
                .ToDictionary(g => g.Key, g => g.ToList());
            OnBuildUtil.RegisterAction(OnPreSingletonBuild, order: -152);
            foreach (System.Type ubType in ubTypes)
                OnBuildUtil.RegisterTypeCumulative(ubType, c => OnSingletonBuild(ubType, c), order: -151);
            OnBuildUtil.RegisterType<UdonSharpBehaviour>(OnBuild, order: -150);
        }

        public static void RegisterCustomDependencyResolver(ISingletonDependencyResolver resolver)
        {
            customDependencyResolvers.Add(resolver);
        }

        /// <summary>
        /// <para>Must be called within a handler for <see cref="OnBuildUtil"/>.</para>
        /// <para>Ensures that the singleton for the given type exists in the scene.</para>
        /// </summary>
        public static void EnsureSingletonExists<T>()
            where T : UdonSharpBehaviour
        {
            if (!singletons.TryGetValue(typeof(T), out SingletonData singletonData))
                throw new System.Exception($"[JanSharpCommon] Attempt to EnsureSingletonExists the type "
                    + $"{typeof(T).Name}, which does not have the {nameof(SingletonScriptAttribute)}.");
            if (!OnBuildUtil.IsRunningOnBuildHandlers)
                throw new System.Exception($"[JanSharpCommon] Attempt to EnsureSingletonExists outside of an "
                    + $"on build handler. Use one of the register functions on the OnBuildUtil and call "
                    + $"EnsureSingletonExists from within a registered callback.");
            if (Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Any(c => !EditorUtil.IsEditorOnly(c)))
            {
                return;
            }
            InstantiatePrefab(singletonData, escalateToException: true);
            if (singletonManager != null)
            {
                // If the OnBuild for the manager already ran, update properties again.
                // This isn't even really needed because instantiating a prefab tells the OnBuildUtil to rerun
                // all on build handlers again, but it just feels right to do this here too.
                UpdateSingletonManagerProperties(singletonManager);
            }
        }

        private static bool OnPreSingletonBuild()
        {
            bool success = true;
            foreach (var singletonKvp in singletons)
            {
                if (AssetDatabase.GUIDToAssetPath(singletonKvp.Value.prefabGuid) != "")
                    continue;
                success = false;
                Debug.LogError($"[JanSharpCommon] The singleton script {singletonKvp.Key.Name} is referencing "
                    + $"a non existent prefab (No asset found with the guid {singletonKvp.Value.prefabGuid}).");
            }
            return success;
        }

        private static bool OnSingletonBuild(System.Type singletonType, ReadOnlyCollection<Component> singletonInstances)
        {
            if (singletonInstances.Count > 1)
            {
                Debug.LogError($"[JanSharpCommon] There must only be 1 instance of the "
                    + $"{singletonType.Name} script in the scene.", singletonInstances.First());
                return false;
            }
            singletons[singletonType].inst = (UdonSharpBehaviour)singletonInstances.FirstOrDefault();
            return true;
        }

        private static bool TryGetTypeCache(System.Type ubType, out List<(string fieldName, System.Type singletonType, bool optional)> cached)
        {
            if (typeCache.TryGetValue(ubType, out cached))
                return true;
            cached = new();

            foreach (var attr in ubType.GetCustomAttributes<SingletonDependencyAttribute>())
            {
                if (!singletons.ContainsKey(attr.SingletonType))
                {
                    Debug.LogError($"[JanSharpCommon] The {ubType.Name} has a {nameof(SingletonDependencyAttribute)} "
                        + $"however there is no {attr.SingletonType.Name} class which has the {nameof(SingletonScriptAttribute)}.");
                    return false;
                }
                cached.Add((null, attr.SingletonType, false));
            }

            foreach (FieldInfo field in EditorUtil.GetFieldsIncludingBase(ubType, PrivateAndPublicFlags, stopAtType: typeof(UdonSharpBehaviour)))
            {
                SingletonReferenceAttribute attr = field.GetCustomAttribute<SingletonReferenceAttribute>(inherit: true);
                if (attr == null)
                    continue;
                if (!singletons.ContainsKey(field.FieldType))
                {
                    Debug.LogError($"[JanSharpCommon] The {ubType.Name}.{field.Name} field has the {nameof(SingletonReferenceAttribute)} "
                        + $"however there is no {field.FieldType.Name} class which has the {nameof(SingletonScriptAttribute)}.");
                    return false;
                }
                if (!EditorUtil.IsSerializedField(field))
                {
                    Debug.LogError($"[JanSharpCommon] The {ubType.Name}.{field.Name} field has the {nameof(SingletonReferenceAttribute)} "
                        + $"however it is not a serialized field. It must either be public or have the {nameof(SerializeField)} attribute.");
                    return false;
                }
                cached.Add((field.Name, field.FieldType, attr.Optional));
            }

            foreach (var resolver in customDependencyResolvers)
                cached.AddRange(resolver.Resolve(ubType));

            typeCache.Add(ubType, cached);
            return true;
        }

        private static bool InstantiatePrefab(SingletonData singleton, bool escalateToException = false)
        {
            bool Error(string msg)
            {
                if (escalateToException)
                    throw new System.Exception(msg);
                else
                    Debug.LogError(msg);
                return false;
            }
            GameObject prefab = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(singleton.prefabGuid)) as GameObject;
            if (prefab == null)
                return Error($"[JanSharpCommon] The prefab guid for the {singleton.singletonType.Name} "
                    + $"singleton references an asset which is not a prefab.\n"
                    + $"'{singleton.prefabGuid}' resolves to: '{AssetDatabase.GUIDToAssetPath(singleton.prefabGuid)}'.");
            GameObject instGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instGo, "Instantiate singleton prefab");
            foreach (SingletonData otherSingleton in singletonsByAssetGuid[singleton.prefabGuid])
            {
                otherSingleton.inst = (UdonSharpBehaviour)instGo.GetComponentInChildren(otherSingleton.singletonType);
                if (otherSingleton.inst == null)
                    return Error($"[JanSharpCommon] The singleton prefab {AssetDatabase.GUIDToAssetPath(otherSingleton.prefabGuid)} "
                        + $"does not contain an UdonSharpBehavior of the type {otherSingleton.singletonType.Name}, "
                        + $"even though the prefab is associated with this singleton script.");
            }
            foreach (UdonSharpBehaviour ub in instGo.GetComponentsInChildren<UdonSharpBehaviour>(includeInactive: true))
                OnBuild(ub);
            OnBuildUtil.MarkForRerunDueToScriptInstantiation();
            return true;
        }

        private static bool OnBuild(UdonSharpBehaviour ub)
        {
            if (!TryGetTypeCache(ub.GetType(), out var cached))
                return false;
            if (!cached.Any())
                return true;

            SerializedObject so = new SerializedObject(ub);
            foreach (var field in cached)
            {
                SingletonData singleton = singletons[field.singletonType];
                if (!field.optional && singleton.inst == null && ub.GetComponentInParent<BypassSingletonDependencyInstantiation>(includeInactive: true) == null)
                    if (!InstantiatePrefab(singleton))
                        return false;
                if (field.fieldName != null)
                    so.FindProperty(field.fieldName).objectReferenceValue = singleton.inst;
            }
            so.ApplyModifiedProperties();

            return true;
        }

        private static bool OnSingletonManagerBuild(SingletonManager manager)
        {
            singletonManager = manager;
            UpdateSingletonManagerProperties(manager);
            if (manager.transform.parent != null)
            {
                Debug.LogError("[JanSharpCommon] The SingletonManager must be in the root of the scene hierarchy.", manager);
                return false;
            }
            if (manager.name != "SingletonManager")
            {
                Debug.LogError("[JanSharpCommon] The SingletonManager's game object must have the exact name 'SingletonManager'.", manager);
                return false;
            }
            return true;
        }

        private static void UpdateSingletonManagerProperties(SingletonManager manager)
        {
            SerializedObject so = new SerializedObject(manager);
            List<SingletonData> existentSingletons = singletonsList.Where(s => s.inst != null).ToList();
            EditorUtil.SetArrayProperty(
                so.FindProperty("singletonInsts"),
                existentSingletons,
                (p, v) => p.objectReferenceValue = v.inst);
            EditorUtil.SetArrayProperty(
                so.FindProperty("singletonClassNames"),
                existentSingletons,
                (p, v) => p.stringValue = v.singletonType.Name);
            so.ApplyModifiedProperties();
        }
    }
}
