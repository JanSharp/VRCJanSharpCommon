using System.Collections.Generic;
using System.Reflection;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class FindInParentOnBuild
    {
        /// <summary>
        /// <para>Contains only entires where <see cref="TypeCache.fields"/> contain at least one value.</para>
        /// </summary>
        private static List<TypeCache> ubTypeCache = new();
        private static List<System.Type> invalidUbTypes = new();
        private const BindingFlags PrivateAndPublicFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private class TypeCache
        {
            public System.Type ubType;
            public List<(string fieldName, System.Type fieldType)> fields = new();

            public TypeCache(System.Type ubType)
            {
                this.ubType = ubType;
            }
        }

        static FindInParentOnBuild()
        {
            ubTypeCache.Clear();
            invalidUbTypes.Clear();
            foreach (System.Type ubType in OnAssemblyLoadUtil.AllUdonSharpBehaviourTypes)
                TryGenerateTypeCache(ubType);
            if (invalidUbTypes.Count != 0)
            {
                OnBuildUtil.RegisterAction(InvalidFindInParentAttributes, order: -1000000);
                return;
            }

            foreach (TypeCache cached in ubTypeCache)
            {
                OnBuildUtil.RegisterType(
                    cached.ubType,
                    ub => OnBuild((UdonSharpBehaviour)ub, cached),
                    order: -10519); // "Random" order.
            }
        }

        private static bool InvalidFindInParentAttributes()
        {
            foreach (System.Type ubType in invalidUbTypes)
                TryGenerateTypeCache(ubType, validateOnly: true);
            return false;
        }

        private static void TryGenerateTypeCache(System.Type ubType, bool validateOnly = false)
        {
            TypeCache cached = validateOnly ? null : new(ubType);

            bool isValid = true;

            foreach (FieldInfo field in EditorUtil.GetFieldsIncludingBase(ubType, PrivateAndPublicFlags, stopAtType: typeof(UdonSharpBehaviour)))
            {
                FindInParentAttribute attr = field.GetCustomAttribute<FindInParentAttribute>(inherit: true);
                if (attr == null)
                    continue;

                if (!EditorUtil.DerivesFrom(field.FieldType, typeof(Component)))
                {
                    Debug.LogError($"[PermissionSystem] The {ubType.Name}.{field.Name} field has the {nameof(FindInParentAttribute)} "
                        + $"however its type is {field.FieldType.Name}. It must be any type deriving from {nameof(Component)}.");
                    isValid = false;
                }
                if (!EditorUtil.IsSerializedField(field))
                {
                    Debug.LogError($"[JanSharpCommon] The {ubType.Name}.{field.Name} field has the {nameof(FindInParentAttribute)} "
                        + $"however it is not a serialized field. It must either be public or have the {nameof(SerializeField)} attribute.");
                    isValid = false;
                }

                if (isValid && !validateOnly)
                    cached.fields.Add((field.Name, field.FieldType));
            }

            if (validateOnly)
                return;

            if (!isValid)
            {
                invalidUbTypes.Add(ubType);
                return;
            }

            if (cached.fields.Count != 0)
                ubTypeCache.Add(cached);
        }

        private static bool OnBuild(UdonSharpBehaviour ub, TypeCache cached)
        {
            SerializedObject so = new(ub);
            foreach (var data in cached.fields)
                so.FindProperty(data.fieldName).objectReferenceValue = ub.GetComponentInParent(data.fieldType, includeInactive: true);
            so.ApplyModifiedProperties();
            return true;
        }
    }
}
