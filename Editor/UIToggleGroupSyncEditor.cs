using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;
using UnityEditor;
using UdonSharpEditor;
using System.Linq;
using System.Collections.Generic;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class UIToggleGroupSyncOnBuild
    {
        static UIToggleGroupSyncOnBuild() => JanSharp.OnBuildUtil.RegisterType<UIToggleGroupSync>(OnBuild);

        private static bool OnBuild(UIToggleGroupSync uiToggleGroupSync)
        {
            SerializedObject proxy = new SerializedObject(uiToggleGroupSync);

            ToggleGroup toggleGroup = (ToggleGroup)proxy.FindProperty("toggleGroup").objectReferenceValue;
            if (toggleGroup == null)
            {
                Debug.LogError($"[JanSharp Common] The Toggle Group must not be null for the "
                    + $"{nameof(UIToggleGroupSync)} {uiToggleGroupSync.name}.", uiToggleGroupSync);
                return false;
            }

            if (!proxy.FindProperty("automaticallyUseChildToggles").boolValue)
            {
                if (!ValidateToggles(uiToggleGroupSync, proxy, toggleGroup))
                    return false;
            }
            else
            {
                if (!UseChildToggles(uiToggleGroupSync, proxy, toggleGroup))
                    return false;
            }

            // From reference: "This method returns -1 if an item that matches the conditions is not found."
            proxy.FindProperty("activeToggleIndex").intValue = EditorUtil.EnumerateArrayProperty(proxy.FindProperty("togglesInGroup"))
                .Select(p => (Toggle)p.objectReferenceValue)
                .ToList()
                .FindIndex(t => t != null && t.isOn);
            proxy.ApplyModifiedProperties();

            return true;
        }

        private static bool ValidateToggles(
            UIToggleGroupSync uiToggleGroupSync,
            SerializedObject proxy,
            ToggleGroup toggleGroup)
        {

            List<Toggle> togglesInGroup = EditorUtil.EnumerateArrayProperty(proxy.FindProperty("togglesInGroup"))
                .Select(p => (Toggle)p.objectReferenceValue)
                .Where(t => t != null)
                .ToList();

            Toggle problemToggle = togglesInGroup.FirstOrDefault(t => t.group != toggleGroup);
            if (problemToggle != null)
            {
                Debug.LogError($"[JanSharp Common] Every toggle in the Toggles In Group array for a "
                    + $"{nameof(UIToggleGroupSync)} must have its Group set to the Toggle Group that's also "
                    + $"referenced on the {nameof(UIToggleGroupSync)} {uiToggleGroupSync.name}.", problemToggle);
                return false;
            }

            UdonBehaviour udonBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(uiToggleGroupSync);
            problemToggle = togglesInGroup.FirstOrDefault(t => !EditorUtil.HasCustomEventListener(
                new SerializedObject(t).FindProperty("onValueChanged"),
                udonBehaviour,
                nameof(UIToggleGroupSync.OnValueChanged)));
            if (problemToggle != null)
            {
                Debug.LogError($"[JanSharp Common] Every toggle in the Toggles In Group array for a "
                    + $"{nameof(UIToggleGroupSync)} must have a OnValueChanged listener targeting the "
                    + $"{nameof(UIToggleGroupSync)} {uiToggleGroupSync.name}.", problemToggle);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Does not call <see cref="SerializedObject.ApplyModifiedProperties"/> on <paramref name="proxy"/>.
        /// </summary>
        /// <param name="uiToggleGroupSync"></param>
        /// <param name="proxy"></param>
        /// <param name="toggleGroup"></param>
        /// <returns></returns>
        private static bool UseChildToggles(
            UIToggleGroupSync uiToggleGroupSync,
            SerializedObject proxy,
            ToggleGroup toggleGroup)
        {
            Toggle[] togglesInGroup = uiToggleGroupSync.GetComponentsInChildren<Toggle>(includeInactive: true);

            Toggle problemToggle = togglesInGroup.FirstOrDefault(t => t.group != null && t.group != toggleGroup);
            if (problemToggle != null)
            {
                Debug.LogError($"[JanSharp Common] A child Toggle of the automatic {nameof(UIToggleGroupSync)} "
                    + $"{uiToggleGroupSync.name} already has a different Toggle Group set. Aborting to avoid"
                    + $"overwriting something unintentionally.", problemToggle);
                return false;
            }

            foreach (Toggle toggle in togglesInGroup.Where(t => t.group != toggleGroup))
            {
                SerializedObject toggleProxy = new SerializedObject(toggle);
                toggleProxy.FindProperty("m_Group").objectReferenceValue = toggleGroup;
                toggleProxy.ApplyModifiedProperties();
            }

            UdonBehaviour udonBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(uiToggleGroupSync);
            foreach (Toggle toggle in togglesInGroup.Where(t => !EditorUtil.HasCustomEventListener(
                new SerializedObject(t).FindProperty("onValueChanged"),
                udonBehaviour,
                nameof(UIToggleGroupSync.OnValueChanged))))
            {
                SerializedObject toggleProxy = new SerializedObject(toggle);
                EditorUtil.AddPersistentSendCustomEventListener(
                    toggleProxy.FindProperty("onValueChanged"),
                    udonBehaviour,
                    nameof(UIToggleGroupSync.OnValueChanged));
                toggleProxy.ApplyModifiedProperties();
            }

            EditorUtil.SetArrayProperty(
                proxy.FindProperty("togglesInGroup"),
                togglesInGroup,
                (p, v) => p.objectReferenceValue = v);

            return true;
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

            // Not using base.OnInspectorGUI() because the togglesInGroup is hidden for automatic ones.

            SerializedObject mainProxy = new SerializedObject(targets);
            EditorGUILayout.PropertyField(mainProxy.FindProperty("toggleGroup"));
            EditorGUILayout.PropertyField(mainProxy.FindProperty("automaticallyUseChildToggles"));
            mainProxy.ApplyModifiedProperties();

            UIToggleGroupSync[] nonAutomatic = targets.Cast<UIToggleGroupSync>()
                .Where(g => !new SerializedObject(g).FindProperty("automaticallyUseChildToggles").boolValue)
                .ToArray();
            if (nonAutomatic.Length != 0)
            {
                mainProxy = new SerializedObject(nonAutomatic);
                EditorGUILayout.PropertyField(mainProxy.FindProperty("togglesInGroup"));
                mainProxy.ApplyModifiedProperties();
            }

            EditorGUILayout.Space();

            EditorUtil.ConditionalButton(
                new GUIContent("Set Toggle Group to itself", $"Use the Toggle Group component that's on the "
                    + $"same GameObject as the {nameof(UIToggleGroupSync)} component."),
                targets.Cast<UIToggleGroupSync>()
                    .Select(g => (proxy: new SerializedObject(g), groupSync: g))
                    .Where(g => g.proxy.FindProperty("toggleGroup").objectReferenceValue == null
                        && g.groupSync.GetComponent<ToggleGroup>() != null),
                groupSyncs => {
                    foreach (var g in groupSyncs)
                    {
                        g.proxy.FindProperty("toggle").objectReferenceValue
                            = g.groupSync.GetComponent<ToggleGroup>();
                        g.proxy.ApplyModifiedProperties();
                    }
                }
            );

            EditorUtil.ConditionalButton(
                new GUIContent("Setup OnValueChanged listeners", "Add the OnValueChanged listener to all the "
                    + "UI Toggle component for this UdonBehaviour to receive the event, which is required."),
                nonAutomatic
                    .SelectMany(g => EditorUtil.EnumerateArrayProperty(new SerializedObject(g).FindProperty("togglesInGroup")),
                        (g, p) => (
                            toggleGroupSync: g,
                            udonBehaviour: UdonSharpEditorUtility.GetBackingUdonBehaviour(g),
                            toggle: (Toggle)p.objectReferenceValue
                        ))
                    .Where(t => t.toggle != null && !EditorUtil.HasCustomEventListener(
                        new SerializedObject(t.toggle).FindProperty("onValueChanged"),
                        t.udonBehaviour,
                        nameof(UIToggleGroupSync.OnValueChanged))),
                tuples => {
                    foreach (var t in tuples)
                    {
                        SerializedObject proxy = new SerializedObject(t.toggle);
                        EditorUtil.AddPersistentSendCustomEventListener(
                            proxy.FindProperty("onValueChanged"),
                            t.udonBehaviour,
                            nameof(UIToggleGroupSync.OnValueChanged));
                        proxy.ApplyModifiedProperties();
                    }
                }
            );

            EditorUtil.ConditionalButton(
                new GUIContent("Set Toggles In Group array using children"),
                nonAutomatic
                    .Select(g => (
                        toggleGroupSync: g,
                        togglesInGroup: EditorUtil.EnumerateArrayProperty(new SerializedObject(g).FindProperty("togglesInGroup"))
                            .Select(p => (Toggle)p.objectReferenceValue)
                            .Where(t => t != null)
                            .ToList(),
                        childToggles: g.GetComponentsInChildren<Toggle>(includeInactive: true)
                    ))
                    .Where(t => t.togglesInGroup.Count != t.childToggles.Length
                        || t.childToggles.Except(t.togglesInGroup).Any()),
                tuples => {
                    foreach (var t in tuples)
                    {
                        SerializedObject proxy = new SerializedObject(t.toggleGroupSync);
                        EditorUtil.SetArrayProperty(
                            proxy.FindProperty("togglesInGroup"),
                            t.childToggles,
                            (p, v) => p.objectReferenceValue = v);
                        proxy.ApplyModifiedProperties();
                    }
                }
            );

            EditorUtil.ConditionalButton(
                new GUIContent("Set Toggle Group reference for all Toggles In Group"),
                nonAutomatic
                    .Select(g => (toggleGroupSync: g, proxy: new SerializedObject(g)))
                    .SelectMany(t => EditorUtil.EnumerateArrayProperty(t.proxy.FindProperty("togglesInGroup")),
                        (t, p) => (
                            toggleGroup: (ToggleGroup)t.proxy.FindProperty("toggleGroup").objectReferenceValue,
                            toggle: (Toggle)p.objectReferenceValue
                        ))
                    .Where(t => t.toggle != null && t.toggle.group != t.toggleGroup),
                tuples => {
                    foreach (var g in tuples.GroupBy(t => t.toggleGroup))
                    {
                        SerializedObject proxy = new SerializedObject(g.Select(t => t.toggle).ToArray());
                        proxy.FindProperty("m_Group").objectReferenceValue = g.Key;
                        proxy.ApplyModifiedProperties();
                    }
                }
            );
        }
    }
}
