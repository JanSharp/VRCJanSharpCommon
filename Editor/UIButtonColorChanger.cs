using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;

namespace JanSharp
{
    public static class UIColorsChanger
    {
        [MenuItem("Tools/Update Selected UI Colors")]
        public static void UpdateUIColors()
        {
            foreach (Selectable selectable in Selection.gameObjects.SelectMany(go => go.GetComponents<Selectable>()))
            {
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
