using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

// NOTE: this should probably be a menu item instead of a component, but for now this works

namespace JanSharp
{
    public class UIButtonColorChanger : MonoBehaviour { }

    #if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIButtonColorChanger))]
    public class UIButtonColorChangerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Update Colors", "Sets all colors on the button using the Normal Color of the button.")))
            {
                foreach (var changer in targets.Cast<UIButtonColorChanger>())
                {
                    Selectable selectable = changer.GetComponent<Selectable>();
                    var color = selectable.colors.normalColor;
                    SerializedObject selectableProxy = new SerializedObject(selectable);
                    selectableProxy.FindProperty("m_Colors.m_NormalColor").colorValue = color;
                    selectableProxy.FindProperty("m_Colors.m_HighlightedColor").colorValue = color * new Color(0.95f, 0.95f, 0.95f);
                    selectableProxy.FindProperty("m_Colors.m_PressedColor").colorValue = color * new Color(0.75f, 0.75f, 0.75f);
                    selectableProxy.FindProperty("m_Colors.m_SelectedColor").colorValue = color * new Color(0.95f, 0.95f, 0.95f);
                    selectableProxy.FindProperty("m_Colors.m_DisabledColor").colorValue = color * new Color(0.75f, 0.75f, 0.75f, 0.5f);
                    selectableProxy.ApplyModifiedProperties();
                }
            }
        }
    }
    #endif
}
