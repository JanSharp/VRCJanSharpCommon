using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    public static class UIToggleEditor
    {
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
