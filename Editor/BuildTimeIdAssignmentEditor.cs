using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace JanSharp.Internal
{
    public static class BuildTimeIdAssignmentEditor
    {
        private class Manager
        {
            public UdonSharpBehaviour managerInst;
            public System.Type managerType;
            public List<Entry> allEntries = new();

            public Manager(System.Type managerType)
            {
                this.managerType = managerType;
            }
        }

        private class UdonSharpBehaviourWithId
        {
            public UdonSharpBehaviour ub;
            public uint id;

            public UdonSharpBehaviourWithId(UdonSharpBehaviour ub, uint id = 0u)
            {
                this.ub = ub;
                this.id = id;
            }
        }

        private class Entry
        {
            public List<UdonSharpBehaviourWithId> entryInsts = new();
            public Manager manager;
            public System.Type entryType;
            public string idsArrayFieldName;
            public string entriesFieldName;
            public string highestIdFieldName;
        }

        private static Dictionary<System.Type, Manager> managers = new();
        private static List<Manager> managersList = new();
        private static Dictionary<System.Type, List<Entry>> associatedEntriesCache = new();
        private const BindingFlags PrivateAndPublicFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
        {
            OnBuildUtil.RegisterAction(PreOnBuild, order: -141);
            OnBuildUtil.RegisterType<UdonSharpBehaviour>(OnBuild, order: -140);
            OnBuildUtil.RegisterAction(PostOnBuild, order: -139);
        }

        private static bool PreOnBuild()
        {
            managers.Clear();
            managersList.Clear();

            static Manager GetManager(System.Type managerType)
            {
                if (managers.TryGetValue(managerType, out Manager manager))
                    return manager;
                manager = new Manager(managerType);
                managers.Add(managerType, manager);
                managersList.Add(manager);
                return manager;
            }

            foreach (var managerType in OnAssemblyLoadUtil.AllUdonSharpBehaviourTypes)
            {
                if (managerType.IsAbstract)
                    continue;
                foreach (FieldInfo field in EditorUtil.GetFieldsIncludingBase(managerType, PrivateAndPublicFlags, stopAtType: typeof(UdonSharpBehaviour)))
                {
                    var attr = field.GetCustomAttribute<BuildTimeIdAssignmentAttribute>(inherit: true);
                    if (attr == null)
                        continue;
                    if (!managerType.IsDefined(typeof(SingletonScriptAttribute), inherit: true))
                    {
                        Debug.LogError($"[JanSharpCommon] A field inside of the class {managerType.Name} has "
                            + $"the {nameof(BuildTimeIdAssignmentAttribute)}, the {managerType.Name} class "
                            + $"does not have the {nameof(SingletonScriptAttribute)}.");
                        return false;
                    }
                    if (!EditorUtil.ValidateArrayField(
                        EditorUtil.MustDeriveFromTypeValidatorFactory(typeof(UdonSharpBehaviour), isArray: true),
                        field,
                        "[JanSharpCommon]",
                        () => EditorUtil.GetFieldWithAttrErrorMessageIntro<BuildTimeIdAssignmentAttribute>(field)))
                    {
                        return false;
                    }

                    FieldInfo idsArrayField = EditorUtil.GetFieldIncludingBase(managerType, attr.IdsArrayFieldName, PrivateAndPublicFlags);
                    if (idsArrayField == null)
                    {
                        Debug.LogError($"[JanSharpCommon] The field {field.DeclaringType.Name}.{field.Name} has the "
                            + $"{nameof(BuildTimeIdAssignmentAttribute)}, however the referenced ids array "
                            + $"field {attr.IdsArrayFieldName} does not exist.");
                        return false;
                    }
                    if (!EditorUtil.ValidateArrayField(
                        EditorUtil.MustBeOfTypeValidatorFactory(typeof(uint), isArray: true),
                        idsArrayField,
                        "[JanSharpCommon]",
                        () => $"The field {field.DeclaringType.Name}.{field.Name} has the "
                            + $"{nameof(BuildTimeIdAssignmentAttribute)}, it is referencing the "
                            + $"ids array field {field.DeclaringType.Name}.{attr.IdsArrayFieldName}"))
                    {
                        return false;
                    }

                    FieldInfo highestIdField = EditorUtil.GetFieldIncludingBase(managerType, attr.HighestIdFieldName, PrivateAndPublicFlags);
                    if (highestIdField == null)
                    {
                        Debug.LogError($"[JanSharpCommon] The field {field.DeclaringType.Name}.{field.Name} has the "
                            + $"{nameof(BuildTimeIdAssignmentAttribute)}, however the {managerType.Name} class "
                            + $"does not have any field with the name {attr.HighestIdFieldName}.");
                        return false;
                    }
                    if (!EditorUtil.ValidateMustBeOfType(
                        typeof(uint),
                        highestIdField,
                        "[JanSharpCommon]",
                        () => $"The field {field.DeclaringType.Name}.{field.Name} has the "
                            + $"{nameof(BuildTimeIdAssignmentAttribute)}, it is referencing the "
                            + $"ids array field {field.DeclaringType.Name}.{attr.HighestIdFieldName}"))
                    {
                        return false;
                    }
                    if (!EditorUtil.ValidateMustBeSerializedByUnity(
                        highestIdField,
                        "[JanSharpCommon]",
                        () => $"The field {field.DeclaringType.Name}.{field.Name} has the "
                            + $"{nameof(BuildTimeIdAssignmentAttribute)}, it is referencing the "
                            + $"ids array field {field.DeclaringType.Name}.{attr.HighestIdFieldName}"))
                    {
                        return false;
                    }

                    Manager manager = GetManager(managerType);
                    manager.allEntries.Add(new Entry()
                    {
                        manager = manager,
                        entryType = field.FieldType.GetElementType(),
                        idsArrayFieldName = attr.IdsArrayFieldName,
                        entriesFieldName = field.Name,
                        highestIdFieldName = attr.HighestIdFieldName,
                    });
                }
            }

            return true;
        }

        private static bool TryGetTypeCache(System.Type ubType, out List<Entry> associatedEntries)
        {
            if (associatedEntriesCache.TryGetValue(ubType, out associatedEntries))
                return true;
            associatedEntries = managersList.SelectMany(m => m.allEntries)
                .Where(e => e.entryType.IsAssignableFrom(ubType))
                .ToList();
            associatedEntriesCache.Add(ubType, associatedEntries);
            return true;
        }

        private static bool OnBuild(UdonSharpBehaviour ub)
        {
            associatedEntriesCache.Clear();
            if (managers.TryGetValue(ub.GetType(), out Manager manager))
            {
                manager.managerInst = ub;
                return true;
            }
            if (ub.GetComponentInParent<BypassBuildTimeIdAssignment>(includeInactive: true) != null)
                return true;
            if (!TryGetTypeCache(ub.GetType(), out var associatedEntries))
                return false;
            foreach (Entry entry in associatedEntries)
                entry.entryInsts.Add(new UdonSharpBehaviourWithId(ub));
            return true;
        }

        private static bool PostOnBuildForManager(Manager manager)
        {
            if (manager.managerInst == null)
                return true;
            SerializedObject managerSo = new SerializedObject(manager.managerInst);
            foreach (Entry entry in manager.allEntries)
            {
                SerializedProperty entriesProp = managerSo.FindProperty(entry.entriesFieldName);
                SerializedProperty idsArrayProp = managerSo.FindProperty(entry.idsArrayFieldName);
                if (entriesProp.arraySize != idsArrayProp.arraySize)
                {
                    Debug.LogError($"[JanSharpCommon] Somebody or something messed with fields managed by "
                        + $"the {nameof(BuildTimeIdAssignmentAttribute)}, the fields {entry.entriesFieldName} "
                        + $"and {entry.idsArrayFieldName} must have the same arraySize.");
                    return false;
                }
                Dictionary<UdonSharpBehaviour, uint> knownEntryInstIdLut = new();
                for (int i = 0; i < entriesProp.arraySize; i++)
                {
                    UdonSharpBehaviour ub = (UdonSharpBehaviour)entriesProp.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (ub == null)
                        continue;
                    uint id = idsArrayProp.GetArrayElementAtIndex(i).uintValue;
                    knownEntryInstIdLut.Add(ub, id);
                }
                SerializedProperty highestIdProp = managerSo.FindProperty(entry.highestIdFieldName);
                uint highestId = highestIdProp.uintValue;
                foreach (UdonSharpBehaviourWithId entryInst in entry.entryInsts)
                    entryInst.id = knownEntryInstIdLut.TryGetValue(entryInst.ub, out uint id)
                        ? id
                        : ++highestId;
                highestIdProp.uintValue = highestId;
                entry.entryInsts = entry.entryInsts.OrderBy(e => e.id).ToList();
                EditorUtil.SetArrayProperty(entriesProp, entry.entryInsts, (p, v) => p.objectReferenceValue = v.ub);
                EditorUtil.SetArrayProperty(idsArrayProp, entry.entryInsts, (p, v) => p.uintValue = v.id);
                managerSo.ApplyModifiedProperties();
            }
            return true;
        }

        private static bool PostOnBuild()
        {
            foreach (Manager manager in managersList)
                if (!PostOnBuildForManager(manager))
                    return false;
            return true;
        }
    }
}
