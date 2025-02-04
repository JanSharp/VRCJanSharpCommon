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
    [DefaultExecutionOrder(-999)]
    public static class SingletonScriptEditor
    {
        private static Dictionary<System.Type, UdonSharpBehaviour> singletons = new();
        private static Dictionary<System.Type, List<(string fieldName, System.Type singletonType, bool optional)>> typeCache = new();
        private const BindingFlags PrivateAndPublicFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        static SingletonScriptEditor()
        {
            singletons.Clear();
            typeCache.Clear();

            IEnumerable<System.Type> ubTypes = OnAssemblyLoadUtil.AllUdonSharpBehaviourTypes
                .Where(t => t.IsDefined(typeof(SingletonScriptAttribute), inherit: false));
            if (!ubTypes.Any())
                return;
            foreach (System.Type ubType in ubTypes)
                OnBuildUtil.RegisterTypeCumulative(ubType, c => OnSingletonBuild(ubType, c), order: -151);
            OnBuildUtil.RegisterType<UdonSharpBehaviour>(OnBuild, order: -150);
        }

        private static bool OnSingletonBuild(System.Type singletonType, ReadOnlyCollection<Component> singletonInstances)
        {
            if (singletonInstances.Count > 1)
            {
                Debug.LogError($"[JanSharpCommon] There must only be 1 instance of the "
                    + $"{singletonType.Name} script in the scene.", singletonInstances.First());
                return false;
            }
            singletons[singletonType] = (UdonSharpBehaviour)singletonInstances.FirstOrDefault();
            return true;
        }

        private static bool TryGetTypeCache(System.Type ubType, out List<(string fieldName, System.Type singletonType, bool optional)> cached)
        {
            if (typeCache.TryGetValue(ubType, out cached))
                return true;
            cached = new();

            foreach (FieldInfo field in EditorUtil.GetFieldsIncludingBase(ubType, PrivateAndPublicFlags, stopAtType: typeof(UdonSharpBehaviour)))
            {
                SingletonReferenceAttribute attr = field.GetCustomAttribute<SingletonReferenceAttribute>(inherit: true);
                if (attr == null)
                    continue;
                if (!singletons.TryGetValue(field.FieldType, out UdonSharpBehaviour singleton))
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

        private static bool OnBuild(UdonSharpBehaviour ub)
        {
            if (!TryGetTypeCache(ub.GetType(), out var cached))
                return false;

            SerializedObject so = new SerializedObject(ub);
            bool isMissingSingletons = false;
            foreach (var field in cached)
            {
                UdonSharpBehaviour singletonInst = singletons[field.singletonType];
                so.FindProperty(field.fieldName).objectReferenceValue = singletonInst;
                if (!field.optional && singletonInst == null && ub.GetComponentInParent<BypassSingletonRequirementValidation>() == null)
                {
                    Debug.LogError($"[JanSharpCommon] The {ub.GetType().Name}.{field.fieldName} is marked as "
                        + $"non optional however there is no {field.singletonType.Name} singleton instance "
                        + $"in the scene.");
                    isMissingSingletons = true;
                }
            }
            so.ApplyModifiedProperties();

            return !isMissingSingletons;
        }
    }
}
