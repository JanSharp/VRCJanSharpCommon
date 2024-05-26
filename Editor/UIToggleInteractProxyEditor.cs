using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using UnityEditor;
using UnityEditor.Events;
using UdonSharpEditor;
using System.Linq;
using System.Collections.Generic;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class UIToggleInteractProxyOnBuild
    {
        static UIToggleInteractProxyOnBuild()
            => JanSharp.OnBuildUtil.RegisterType<UIToggleInteractProxy>(OnBuild);

        private static bool OnBuild(UIToggleInteractProxy uiToggleInteractProxy)
        {
            SerializedObject proxy = new SerializedObject(uiToggleInteractProxy);

            Toggle toggle = (Toggle)proxy.FindProperty("toggle").objectReferenceValue;
            if (toggle == null)
            {
                Debug.LogError($"[JanSharp Common] The Toggle must not be null for the "
                    + $"{nameof(UIToggleInteractProxy)} {uiToggleInteractProxy.name}.", uiToggleInteractProxy);
                return false;
            }

            proxy.FindProperty("wasOn").boolValue = toggle.isOn;
            proxy.ApplyModifiedProperties();

            return true;
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

            EditorUtil.ConditionalButton(
                new GUIContent("Set Toggle to itself", $"Use the Toggle component that's on the same "
                    + $"GameObject as the {nameof(UIToggleInteractProxy)} component."),
                targets.Cast<UIToggleInteractProxy>()
                    .Select(p => (proxy: new SerializedObject(p), interactProxy: p))
                    .Where(p => p.proxy.FindProperty("toggle").objectReferenceValue == null
                        && p.interactProxy.GetComponent<Toggle>() != null),
                proxies => {
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
                targets.Cast<UIToggleInteractProxy>(),
                p => (Toggle)new SerializedObject(p).FindProperty("toggle").objectReferenceValue,
                "onValueChanged",
                nameof(UIToggleInteractProxy.OnValueChanged)
            );
        }
    }
}
