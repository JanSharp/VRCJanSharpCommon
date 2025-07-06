using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class UIToggleProxyOnBuild
    {
        static UIToggleProxyOnBuild()
        {
            JanSharp.OnBuildUtil.RegisterType<UIToggleInteractProxy>(p => OnBuild(p));
            JanSharp.OnBuildUtil.RegisterType<UIToggleSendLocalEvent>(p => OnBuild(p));
        }

        private static bool OnBuild<T>(T uiToggleProxy)
            where T : UdonSharpBehaviour
        {
            SerializedObject proxy = new SerializedObject(uiToggleProxy);

            Toggle toggle = (Toggle)proxy.FindProperty("toggle").objectReferenceValue;
            if (toggle == null)
            {
                Debug.LogError($"[JanSharpCommon] The Toggle must not be null for the "
                    + $"{typeof(T).Name} {uiToggleProxy.name}.", uiToggleProxy);
                return false;
            }

            proxy.FindProperty("wasOn").boolValue = toggle.isOn;
            proxy.ApplyModifiedProperties();

            return true;
        }
    }

    internal static class UIToggleProxyEditorUtil
    {
        public static void DrawConditionalButtons<T>(IEnumerable<T> targets)
            where T : UdonSharpBehaviour
        {
            EditorUtil.ConditionalButton(
                new GUIContent("Set Toggle to itself", $"Use the Toggle component that's on the same "
                    + $"GameObject as the {typeof(T).Name} component."),
                targets.Select(p => (proxy: new SerializedObject(p), interactProxy: p))
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

            EditorUtil.ConditionalRegisterCustomEventListenerButton(
                new GUIContent("Setup OnValueChanged listener", "Add the OnValueChanged listener to the "
                    + "UI Toggle component for this UdonBehaviour to receive the event, which is required."),
                targets,
                p => (Toggle)new SerializedObject(p).FindProperty("toggle").objectReferenceValue,
                "onValueChanged",
                nameof(UIToggleInteractProxy.OnValueChanged)
            // Or this, also works, same thing:
            // nameof(UIToggleSendLocalEvent.OnValueChanged)
            );
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIToggleInteractProxy))]
    public class UIToggleInteractProxyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;
            EditorGUILayout.Space();
            base.OnInspectorGUI(); // draws public/serializable fields
            EditorGUILayout.Space();

            UIToggleProxyEditorUtil.DrawConditionalButtons(targets.Cast<UIToggleInteractProxy>());
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIToggleSendLocalEvent))]
    public class UIToggleSendLocalEventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;
            EditorGUILayout.Space();
            base.OnInspectorGUI(); // draws public/serializable fields
            EditorGUILayout.Space();

            UIToggleProxyEditorUtil.DrawConditionalButtons(targets.Cast<UIToggleSendLocalEvent>());
        }
    }
}
