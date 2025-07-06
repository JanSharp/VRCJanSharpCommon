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
    public static class UIToggleEditor
    {
        static UIToggleEditor()
        {
            // Specifically -204 because it's random to not conflict with other scripts wanting all toggles.
            OnBuildUtil.RegisterType<Toggle>(OnToggleBuild, order: -204);
        }

        private static bool OnToggleBuild(Toggle toggle)
        {
            SerializedObject so = new SerializedObject(toggle);
            SerializedProperty onValueChangedProperty = so.FindProperty("onValueChanged");
            List<int> toRemove = new();
            bool foundOneValidForToggleSync = false;
            bool foundOneValidForGroupSync = false;
            foreach (var l in EditorUtil.EnumeratePersistentEventListeners(onValueChangedProperty)
                .Select((listener, index) => (listener, index)))
            {
                if (IsInvalidForToggleSync(l.listener, toggle, ref foundOneValidForToggleSync)
                    || IsInvalidForToggleGroupSync(l.listener, toggle, ref foundOneValidForGroupSync))
                {
                    toRemove.Add(l.index);
                }
            }
            toRemove.Reverse();
            foreach (int index in toRemove)
                EditorUtil.DeletePersistentEventListenerAtIndex(onValueChangedProperty, index);
            so.ApplyModifiedProperties();
            return true;
        }

        private static Toggle GetToggle(UIToggleSync toggleSync)
        {
            SerializedObject so = new SerializedObject(toggleSync);
            return (Toggle)so.FindProperty("toggle").objectReferenceValue;
        }

        private static bool IsInvalidForToggleSync(EditorUtil.PersistentEventListenerWrapper listener, Toggle toggle, ref bool foundOneValidOne)
        {
            if (listener.Target is not UdonBehaviour targetBehaviour
                || UdonSharpEditorUtility.GetProxyBehaviour(targetBehaviour) is not UIToggleSync toggleSync)
                return false;
            if (!foundOneValidOne
                && toggle == GetToggle(toggleSync)
                && listener.MethodName == nameof(UdonBehaviour.SendCustomEvent)
                && listener.StringArgument == nameof(UIToggleSync.OnValueChanged))
            {
                foundOneValidOne = true;
                return false;
            }
            return true;
        }

        private static ToggleGroup GetToggleGroup(UIToggleGroupSync groupSync)
        {
            SerializedObject so = new SerializedObject(groupSync);
            return (ToggleGroup)so.FindProperty("toggleGroup").objectReferenceValue;
        }

        private static bool IsInvalidForToggleGroupSync(EditorUtil.PersistentEventListenerWrapper listener, Toggle toggle, ref bool foundOneValidOne)
        {
            if (listener.Target is not UdonBehaviour targetBehaviour
                || UdonSharpEditorUtility.GetProxyBehaviour(targetBehaviour) is not UIToggleGroupSync groupSync)
                return false;
            if (!foundOneValidOne
                && toggle.group == GetToggleGroup(groupSync)
                && listener.MethodName == nameof(UdonBehaviour.SendCustomEvent)
                && listener.StringArgument == nameof(UIToggleGroupSync.OnValueChanged))
            {
                foundOneValidOne = true;
                return false;
            }
            return true;
        }

        [MenuItem("Tools/JanSharp/Remove UI Toggle Listeners Targeting Missing Objects", priority = 10003)]
        public static void RemoveUIToggleListenersTargetingMissingObjects()
        {
            foreach (Toggle toggle in Object.FindObjectsByType<Toggle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                RemoveListeners(toggle);
        }

        private static void RemoveListeners(Toggle toggle)
        {
            SerializedObject so = new SerializedObject(toggle);
            SerializedProperty onValueChangedProperty = so.FindProperty("onValueChanged");
            foreach (var l in EditorUtil.EnumeratePersistentEventListeners(onValueChangedProperty)
                .Select((listener, index) => (listener, index))
                .Where(l => l.listener.Target == null)
                .Reverse())
            {
                EditorUtil.DeletePersistentEventListenerAtIndex(onValueChangedProperty, l.index);
            }
            so.ApplyModifiedProperties();
        }
    }
}
