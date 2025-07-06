using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class UIToggleSyncOnBuild
    {
        static UIToggleSyncOnBuild() => OnBuildUtil.RegisterType<UIToggleSync>(OnBuild);

        private static bool OnBuild(UIToggleSync uiToggleSync)
        {
            SerializedObject so = new SerializedObject(uiToggleSync);

            Toggle toggle = (Toggle)so.FindProperty("toggle").objectReferenceValue;
            if (toggle == null)
            {
                Debug.LogError($"[JanSharpCommon] The Toggle must not be null for the "
                    + $"{nameof(UIToggleSync)} {uiToggleSync.name}.", uiToggleSync);
                return false;
            }

            SetupOnValueChangedListener(uiToggleSync, toggle);

            so.FindProperty("isOn").boolValue = toggle.isOn;
            so.ApplyModifiedProperties();

            return true;
        }

        private static void SetupOnValueChangedListener(UIToggleSync uiToggleSync, Toggle toggle)
        {
            SerializedObject so = new SerializedObject(toggle);
            SerializedProperty onValueChangedProperty = so.FindProperty("onValueChanged");
            UdonBehaviour udonBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(uiToggleSync);

            if (EditorUtil.HasCustomEventListener(
                onValueChangedProperty,
                udonBehaviour,
                nameof(UIToggleGroupSync.OnValueChanged)))
            {
                return;
            }

            EditorUtil.AddPersistentSendCustomEventListener(
                onValueChangedProperty,
                udonBehaviour,
                nameof(UIToggleGroupSync.OnValueChanged));
            so.ApplyModifiedProperties();
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIToggleSync))]
    public class UIToggleSyncEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;
            EditorGUILayout.Space();
            base.OnInspectorGUI(); // draws public/serializable fields

            EditorUtil.ConditionalButton(
                new GUIContent("Set Toggle to itself", $"Use the Toggle component that's on the same "
                    + $"GameObject as the {nameof(UIToggleSync)} component."),
                targets.Cast<UIToggleSync>()
                    .Select(p => (proxy: new SerializedObject(p), interactProxy: p))
                    .Where(p => p.proxy.FindProperty("toggle").objectReferenceValue == null
                        && p.interactProxy.GetComponent<Toggle>() != null),
                proxies =>
                {
                    foreach (var p in proxies)
                    {
                        p.proxy.FindProperty("toggle").objectReferenceValue
                            = p.interactProxy.GetComponent<Toggle>();
                        p.proxy.ApplyModifiedProperties();
                    }
                }
            );

            EditorGUILayout.Space();
            GUILayout.Label("Automatically sets up an OnValueChanged listener on the UI Toggle "
                + "upon entering play mode and on build.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            GUILayout.Label("Use 'Tools -> JanSharp -> Remove UI Toggle Listeners Targeting Missing Objects' "
                + "to remove stray listeners on UI Toggles after deleting a UI Toggle Sync script.",
                EditorStyles.wordWrappedLabel);
        }
    }
}
