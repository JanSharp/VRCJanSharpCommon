using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace JanSharp
{
    public static class FindInSceneOrChildrenOnBuild
    {
        /// <summary>
        /// <para>Contains only entires where <see cref="UBTypeCache.findInSceneFields"/> contain at least one value.</para>
        /// </summary>
        private static List<UBTypeCache> ubTypeCache = new();
        private static List<System.Type> invalidUbTypes = new();

        private static Dictionary<System.Type, List<FieldInfo>> findInSceneFieldsByDeclaringType = new();
        private static Dictionary<System.Type, List<FieldInfo>> findInChildrenFieldsByDeclaringType = new();
        private static Dictionary<System.Type, List<Component>> foundComponentsByType = new();

        private class UBTypeCache
        {
            public System.Type ubType;
            /// <summary>
            /// <para><c>fieldType</c> refers to the type of entries in the array, not the full array type.</para>
            /// </summary>
            public List<(string fieldName, System.Type fieldType)> findInSceneFields = new();
            /// <inheritdoc cref="findInSceneFields"/>
            public List<(string fieldName, System.Type fieldType)> findInChildrenFields = new();

            public UBTypeCache(System.Type ubType)
            {
                this.ubType = ubType;
            }
        }

        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
        {
            findInSceneFieldsByDeclaringType = CreateLutForAttribute<FindInSceneAttribute>();
            findInChildrenFieldsByDeclaringType = CreateLutForAttribute<FindInChildrenAttribute>();

            ubTypeCache.Clear();
            invalidUbTypes.Clear();
            foreach (System.Type ubType in OnAssemblyLoadUtil.AllUdonSharpBehaviourTypes)
                TryGenerateTypeCache(ubType);
            if (invalidUbTypes.Count != 0)
            {
                OnBuildUtil.RegisterAction(InvalidFindInSceneOrChildrenAttributes, order: -1000000);
                return;
            }

            if (ubTypeCache.Count == 0)
                return;

            OnBuildUtil.RegisterAction(OnPreBuild, order: -10473); // "Random" order.

            foreach (System.Type findInSceneType in ubTypeCache
                .Where(c => c.findInSceneFields.Count != 0)
                .SelectMany(c => c.findInSceneFields)
                .Select(f => f.fieldType)
                .Distinct())
            {
                OnBuildUtil.RegisterTypeCumulative(
                    findInSceneType,
                    components => OnFindInSceneOnBuild(findInSceneType, components),
                    order: -10472); // "Random" order.
            }

            foreach (UBTypeCache cached in ubTypeCache)
            {
                OnBuildUtil.RegisterType(
                    cached.ubType,
                    ub => OnBuild((UdonSharpBehaviour)ub, cached),
                    order: -10471, // "Random" order.
                    includeEditorOnly: true); // Specifically to clear arrays for cleanliness when using tools like the references window.
            }

            OnBuildUtil.RegisterAction(OnPostBuild, order: -10470); // "Random" order.
        }

        private static bool InvalidFindInSceneOrChildrenAttributes()
        {
            foreach (System.Type type in invalidUbTypes)
                TryGenerateTypeCache(type, validateOnly: true);
            return false;
        }

        private static Dictionary<System.Type, List<FieldInfo>> CreateLutForAttribute<T>()
            where T : System.Attribute
        {
            return TypeCache.GetFieldsWithAttribute<T>()
                .GroupBy(f => f.DeclaringType)
                .Where(g => !g.Key.IsAbstract)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private static bool ValidateAndAddFields<TAttribute>(
            System.Type currentTypeToCheck,
            bool validateOnly,
            Dictionary<System.Type, List<FieldInfo>> fieldsByDeclaringType,
            List<(string fieldName, System.Type fieldType)> toSaveIn)
            where TAttribute : System.Attribute
        {
            if (!fieldsByDeclaringType.TryGetValue(currentTypeToCheck, out List<FieldInfo> fields))
                return true;

            bool isValid = true;

            foreach (FieldInfo field in fields)
            {
                isValid &= EditorUtil.ValidateArrayField(
                    EditorUtil.MustDeriveFromTypeValidatorFactory(typeof(Component), isArray: true),
                    field,
                    "[JanSharpCommon]",
                    () => EditorUtil.GetFieldWithAttrErrorMessageIntro(field, typeof(TAttribute)));

                if (isValid && !validateOnly)
                    toSaveIn.Add((field.Name, field.FieldType.GetElementType()));
            }

            return isValid;
        }

        private static void TryGenerateTypeCache(System.Type ubType, bool validateOnly = false)
        {
            UBTypeCache cached = validateOnly ? null : new(ubType);

            bool isValid = true;

            System.Type currentType = ubType;
            while (currentType != typeof(UdonSharpBehaviour))
            {
                isValid &= ValidateAndAddFields<FindInSceneAttribute>(
                    currentType,
                    validateOnly,
                    findInSceneFieldsByDeclaringType,
                    cached?.findInSceneFields);
                isValid &= ValidateAndAddFields<FindInChildrenAttribute>(
                    currentType,
                    validateOnly,
                    findInChildrenFieldsByDeclaringType,
                    cached?.findInChildrenFields);
                currentType = currentType.BaseType;
            }

            if (validateOnly)
                return;

            if (!isValid)
            {
                invalidUbTypes.Add(ubType);
                return;
            }

            if (cached.findInSceneFields.Count != 0 || cached.findInChildrenFields.Count != 0)
                ubTypeCache.Add(cached);
        }

        private static bool OnPreBuild()
        {
            // OnPostBuild would not suffice, on build handlers may stop or restart midway through.
            foundComponentsByType.Clear();
            return true;
        }

        private static bool OnFindInSceneOnBuild(System.Type findInSceneType, IEnumerable<Component> foundComponents)
        {
            foundComponentsByType.Add(findInSceneType, foundComponents.ToList());
            return true;
        }

        private static void ClearArrayProperties(UdonSharpBehaviour ub, UBTypeCache cached)
        {
            SerializedObject so = new(ub);

            foreach (var data in cached.findInSceneFields)
                so.FindProperty(data.fieldName).arraySize = 0;

            foreach (var data in cached.findInChildrenFields)
                so.FindProperty(data.fieldName).arraySize = 0;

            so.ApplyModifiedProperties();
        }

        private static void PopulateArrayProperties(UdonSharpBehaviour ub, UBTypeCache cached)
        {
            SerializedObject so = new(ub);

            foreach (var data in cached.findInSceneFields)
            {
                EditorUtil.SetArrayProperty(
                    so.FindProperty(data.fieldName),
                    foundComponentsByType[data.fieldType],
                    (p, v) => p.objectReferenceValue = v);
            }

            foreach (var data in cached.findInChildrenFields)
            {
                EditorUtil.SetArrayProperty(
                    so.FindProperty(data.fieldName),
                    ub.GetComponentsInChildren(data.fieldType, includeInactive: true)
                        .Where(c => !EditorUtil.IsEditorOnly(c))
                        .ToList(),
                    (p, v) => p.objectReferenceValue = v);
            }

            so.ApplyModifiedProperties();
        }

        private static bool OnBuild(UdonSharpBehaviour ub, UBTypeCache cached)
        {
            if (EditorUtil.IsEditorOnly(ub))
                ClearArrayProperties(ub, cached);
            else
                PopulateArrayProperties(ub, cached);
            return true;
        }

        private static bool OnPostBuild()
        {
            // Just to free up memory.
            foundComponentsByType.Clear();
            return true;
        }
    }
}
