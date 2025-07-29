using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class WannaBeClassesEditor
    {
        private static WannaBeClassesManager manager = null;
        private static List<(string name, System.Type type)> wannaBeClassTypes = null;

        static WannaBeClassesEditor()
        {
            wannaBeClassTypes = null;
            ValidateWannaBeClasses();
            OnBuildUtil.RegisterTypeCumulative<WannaBeClassesManager>(OnManagerBuild, order: 0);
            OnBuildUtil.RegisterTypeCumulative<WannaBeClass>(OnClassInstancesBuild, order: 1);
        }

        private static bool ValidateWannaBeClasses()
        {
            wannaBeClassTypes ??= OnAssemblyLoadUtil.AllUdonSharpBehaviourTypes
                .Where(t => EditorUtil.DerivesFrom(t, typeof(WannaBeClass)) && !t.IsAbstract)
                .Select(t => (name: t.Name, type: t))
                .OrderBy(t => t.name)
                .ToList();
            var duplicates = wannaBeClassTypes.GroupBy(t => t.name).Where(g => g.Count() > 1);
            if (duplicates.Any())
            {
                foreach (var duplicate in duplicates)
                    Debug.LogError($"[JanSharpCommon] There are {duplicate.Count()} WannaBeClasses with the name '{duplicate.Key}', "
                        + $"however each WannaBeClass name must be unique, even if they are in different namespaces. Duplicates:\n"
                        + string.Join('\n', duplicate.Select(d => d.type.FullName)));
                return false;
            }
            return true;
        }

        private static bool OnManagerBuild(IEnumerable<WannaBeClassesManager> managers)
        {
            manager = managers.FirstOrDefault();
            if (manager == null)
                return true;
            if (managers.Skip(1).Any())
            {
                Debug.LogError("[JanSharpCommon] Impossible because the singleton editor script should have "
                    + "already aborted the on build process before we even reach this point. For the record, "
                    + "there are multiple WannaBeClassesManagers which is not allowed.");
                return false;
            }
            if (!ValidateWannaBeClasses())
                return false;
            foreach (var child in manager.prefabsParent.Cast<Transform>().Where(t => t.GetComponent<WannaBeClass>() == null).ToList())
                Undo.DestroyObjectImmediate(child.gameObject);
            var existingPrefabs = manager.prefabsParent.Cast<Transform>()
                .Select(t => t.GetComponent<WannaBeClass>())
                .Select(c => (name: c.GetType().Name, inst: c))
                .ToList();
            foreach (var toRename in existingPrefabs.Where(p => p.name != p.inst.name))
            {
                SerializedObject prefabSo = new SerializedObject(toRename.inst.gameObject);
                prefabSo.FindProperty("m_Name").stringValue = toRename.name;
                prefabSo.ApplyModifiedProperties();
            }
            // If I were to want perfect Undo support then this would be required. However this call also marks
            // the scene as dirty unconditionally, even when the order of children ends up not changing at all.
            // Which is really annoying for this here since it is an OnBuild handler which runs automatically
            // when entering play mode, and I'd rather not mark the scene as dirty every time play mode is
            // entered even when nothing changes.
            // Undo.RegisterChildrenOrderUndo(manager.prefabsParent, "Generate WannaBeClass Prefabs");
            Dictionary<string, WannaBeClass> existingPrefabsLut = existingPrefabs.ToDictionary(p => p.name, p => p.inst);
            for (int i = 0; i < wannaBeClassTypes.Count; i++)
            {
                var wannaBeClassType = wannaBeClassTypes[i];
                if (existingPrefabsLut.TryGetValue(wannaBeClassType.name, out WannaBeClass existing))
                {
                    existing.transform.SetSiblingIndex(i);
                    continue;
                }
                GameObject newPrefab = new GameObject(wannaBeClassType.name);
                Undo.RegisterCreatedObjectUndo(newPrefab, "Generate WannaBeClass Prefabs");
                newPrefab.transform.SetParent(manager.prefabsParent, worldPositionStays: false);
                newPrefab.transform.SetSiblingIndex(i);
                UdonSharpUndo.AddComponent(newPrefab, wannaBeClassType.type);
                OnBuildUtil.MarkForRerunDueToScriptInstantiation();
            }
            SerializedObject so = new SerializedObject(manager);
            EditorUtil.SetArrayProperty(so.FindProperty("wannaBeClassNames"), wannaBeClassTypes, (p, v) => p.stringValue = v.name);
            EditorUtil.SetArrayProperty(so.FindProperty("wannaBeClassPrefabs"), manager.prefabsParent.Cast<Transform>().ToList(), (p, v) => p.objectReferenceValue = v.gameObject);
            so.ApplyModifiedProperties();
            return true;
        }

        private static bool OnClassInstancesBuild(IEnumerable<WannaBeClass> instances)
        {
            if (manager == null)
                return true;
            SerializedObject so = new SerializedObject(manager);
            EditorUtil.SetArrayProperty(
                so.FindProperty("instancesExistingAtBuildTime"),
                instances.Where(i => i.transform.parent != manager.prefabsParent).ToList(),
                (p, v) => p.objectReferenceValue = v);
            so.ApplyModifiedProperties();
            return true;
        }
    }
}
