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
        private static Dictionary<System.Type, List<(string fieldName, System.Type singletonType)>> typeCache = new();
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

        private static IEnumerable<FieldInfo> GetFieldsIncludingBase(System.Type ubType)
        {
            HashSet<string> visitedNames = new HashSet<string>();
            while (ubType != typeof(UdonSharpBehaviour))
            {
                foreach (FieldInfo field in ubType.GetFields(PrivateAndPublicFlags))
                {
                    if (visitedNames.Contains(field.Name))
                        continue;
                    visitedNames.Add(field.Name);
                    yield return field;
                }
                ubType = ubType.BaseType;
            }
        }

        private static bool TryGetTypeCache(System.Type ubType, out List<(string fieldName, System.Type singletonType)> cached)
        {
            if (typeCache.TryGetValue(ubType, out cached))
                return true;
            cached = new();

            foreach (FieldInfo field in GetFieldsIncludingBase(ubType)
                .Where(f => f.IsDefined(typeof(SingletonReferenceAttribute), inherit: true)))
            {
                if (!singletons.TryGetValue(field.FieldType, out UdonSharpBehaviour singleton))
                {
                    Debug.LogError($"[JanSharpCommon] The {ubType.Name}.{field.Name} field has the {nameof(SingletonReferenceAttribute)} "
                        + $"However there is no {field.FieldType.Name} class which has the {nameof(SingletonScriptAttribute)}.");
                    return false;
                }
                if (!EditorUtil.IsSerializedField(field))
                {
                    Debug.LogError($"[JanSharpCommon] The {ubType.Name}.{field.Name} field has the {nameof(SingletonReferenceAttribute)} "
                        + $"However it is not a serialized field. It must either be public or have the {nameof(SerializeField)} attribute.");
                    return false;
                }
                cached.Add((field.Name, field.FieldType));
            }

            typeCache.Add(ubType, cached);
            return true;
        }

        private static bool OnBuild(UdonSharpBehaviour ub)
        {
            if (!TryGetTypeCache(ub.GetType(), out var cached))
                return false;

            foreach (var field in cached)
            {
                SerializedObject so = new SerializedObject(ub);
                so.FindProperty(field.fieldName).objectReferenceValue = singletons[field.singletonType];
                so.ApplyModifiedProperties();
            }

            return true;
        }
    }
}
