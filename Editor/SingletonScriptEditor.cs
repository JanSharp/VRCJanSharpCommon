using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UdonSharp;
using System.Reflection;

namespace JanSharp.Internal
{
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

        private static Dictionary<System.Type, SingletonData> singletons = new();
        private static List<SingletonData> singletonsList = new();
        /// <summary>
        /// <para>Not just fields, also includes dependencies, which simply have null field names.</para>
        /// </summary>
        private static Dictionary<System.Type, List<(string fieldName, System.Type singletonType, bool optional)>> typeCache = new();
        private const BindingFlags PrivateAndPublicFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        static SingletonScriptEditor()
        {
            singletons.Clear();
            singletonsList.Clear();
            typeCache.Clear();

            IEnumerable<System.Type> ubTypes = OnAssemblyLoadUtil.AllUdonSharpBehaviourTypes
                .Where(t => t.IsDefined(typeof(SingletonScriptAttribute), inherit: false));
            if (!ubTypes.Any())
                return;
            singletonsList = ubTypes
                .Select(t => new SingletonData(t, null, t.GetCustomAttribute<SingletonScriptAttribute>().PrefabGuid))
                .ToList();
            singletons = singletonsList.ToDictionary(s => s.singletonType, s => s);
            OnBuildUtil.RegisterAction(OnPreSingletonBuild, order: -152);
            foreach (System.Type ubType in ubTypes)
                OnBuildUtil.RegisterTypeCumulative(ubType, c => OnSingletonBuild(ubType, c), order: -151);
            OnBuildUtil.RegisterType<UdonSharpBehaviour>(OnBuild, order: -150);
            OnBuildUtil.RegisterType<SingletonManager>(OnSingletonManagerBuild, order: -149);
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

            typeCache.Add(ubType, cached);
            return true;
        }

        private static bool InstantiatePrefab(SingletonData singleton)
        {
            Object prefab = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(singleton.prefabGuid));
            Object instObj = PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instObj, "Instantiate singleton prefab");
            GameObject instGo = (GameObject)instObj;
            singleton.inst = (UdonSharpBehaviour)instGo.GetComponentInChildren(singleton.singletonType);
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
                if (!field.optional && singleton.inst == null && ub.GetComponentInParent<BypassSingletonDependencyInstantiation>() == null)
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
    }
}
