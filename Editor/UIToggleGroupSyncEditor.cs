using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class UIToggleGroupSyncOnBuild
    {
        private static Dictionary<ToggleGroup, List<Toggle>> togglesByGroup;

        static UIToggleGroupSyncOnBuild()
        {
            // Specifically -104 because it's random to not conflict with other scripts wanting all toggles.
            OnBuildUtil.RegisterTypeCumulative<Toggle>(OnPreBuild, order: -104);
            OnBuildUtil.RegisterType<UIToggleGroupSync>(OnBuild, order: -10);
        }

        private static bool OnPreBuild(IEnumerable<Toggle> toggles)
        {
            togglesByGroup = toggles
                .Where(t => t.group != null)
                .GroupBy(t => t.group)
                .ToDictionary(g => g.Key, g => g.ToList());
            return true;
        }

        private static bool OnBuild(UIToggleGroupSync uiToggleGroupSync)
        {
            SerializedObject so = new SerializedObject(uiToggleGroupSync);

            ToggleGroup toggleGroup = (ToggleGroup)so.FindProperty("toggleGroup").objectReferenceValue;
            if (toggleGroup == null)
            {
                Debug.LogError($"[JanSharpCommon] The Toggle Group must not be null for the "
                    + $"{nameof(UIToggleGroupSync)} {uiToggleGroupSync.name}.", uiToggleGroupSync);
                return false;
            }

            List<Toggle> togglesInGroup = togglesByGroup.TryGetValue(toggleGroup, out var toggles) ? toggles : new();
            EditorUtil.SetArrayProperty(
                so.FindProperty("togglesInGroup"),
                togglesInGroup,
                (p, v) => p.objectReferenceValue = v);

            SetupOnValueChangedListeners(uiToggleGroupSync, togglesInGroup);

            // From reference: "This method returns -1 if an item that matches the conditions is not found."
            so.FindProperty("activeToggleIndex").intValue = togglesInGroup.FindIndex(t => t != null && t.isOn);
            so.ApplyModifiedProperties();

            return true;
        }

        private static void SetupOnValueChangedListeners(UIToggleGroupSync uiToggleGroupSync, List<Toggle> togglesInGroup)
        {
            UdonBehaviour udonBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(uiToggleGroupSync);
            foreach (Toggle toggle in togglesInGroup.Where(t => !EditorUtil.HasCustomEventListener(
                new SerializedObject(t).FindProperty("onValueChanged"),
                udonBehaviour,
                nameof(UIToggleGroupSync.OnValueChanged))))
            {
                SerializedObject toggleSo = new SerializedObject(toggle);
                EditorUtil.AddPersistentSendCustomEventListener(
                    toggleSo.FindProperty("onValueChanged"),
                    udonBehaviour,
                    nameof(UIToggleGroupSync.OnValueChanged));
                toggleSo.ApplyModifiedProperties();
            }
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIToggleGroupSync))]
    public class UIToggleGroupSyncEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;
            EditorGUILayout.Space();
            base.OnInspectorGUI(); // draws public/serializable fields

            EditorUtil.ConditionalButton(
                new GUIContent("Set Toggle Group to itself", $"Use the Toggle Group component that's on the "
                    + $"same GameObject as the {nameof(UIToggleGroupSync)} component."),
                targets.Cast<UIToggleGroupSync>()
                    .Select(g => (go: new SerializedObject(g), groupSync: g))
                    .Where(g => g.go.FindProperty("toggleGroup").objectReferenceValue == null
                        && g.groupSync.GetComponent<ToggleGroup>() != null),
                groupSyncs =>
                {
                    foreach (var g in groupSyncs)
                    {
                        g.go.FindProperty("toggleGroup").objectReferenceValue
                            = g.groupSync.GetComponent<ToggleGroup>();
                        g.go.ApplyModifiedProperties();
                    }
                }
            );

            EditorGUILayout.Space();
            GUILayout.Label("Automatically finds Toggles that are in the given Group and "
                + "sets up OnValueChanged listeners upon entering play mode and on build.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            GUILayout.Label("Use 'Tools -> JanSharp -> Remove UI Toggle Listeners Targeting Missing Objects' "
                + "to remove stray listeners on UI Toggles after deleting a UI Toggle Group Sync script.",
                EditorStyles.wordWrappedLabel);
        }
    }
}
