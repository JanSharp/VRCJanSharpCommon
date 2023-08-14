using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using System.Linq;

namespace JanSharp
{
    public static class EditorUtil
    {
        public static void SetArrayProperty<T>(SerializedProperty property, ICollection<T> newValues, System.Action<SerializedProperty, T> setValue)
        {
            property.ClearArray();
            property.arraySize = newValues.Count;
            int i = 0;
            foreach (T value in newValues)
                setValue(property.GetArrayElementAtIndex(i++), value);
        }

        public static void AppendProperty(SerializedProperty property, System.Action<SerializedProperty> setNewValue)
        {
            property.InsertArrayElementAtIndex(property.arraySize);
            setNewValue(property.GetArrayElementAtIndex(property.arraySize - 1));
        }

        public static IEnumerable<SerializedProperty> EnumerateArrayProperty(SerializedProperty property)
        {
            for (int i = 0; i < property.arraySize; i++)
                yield return property.GetArrayElementAtIndex(i);
        }

        public struct PersistentEventListenerWrapper
        {
            /* (UI Toggle onValueChanged)
              onValueChanged:
                m_PersistentCalls:
                  m_Calls:
                  - m_Target: {fileID: 2039029658}
                    m_MethodName: SendCustomEvent
                    m_Mode: 5
                    m_Arguments:
                      m_ObjectArgument: {fileID: 0}
                      m_ObjectArgumentAssemblyTypeName: 
                      m_IntArgument: 0
                      m_FloatArgument: 0
                      m_StringArgument: OnValueChanged
                      m_BoolArgument: 0
                    m_CallState: 2
            */

            private UnityEventBase unityEvent;
            private SerializedProperty unityEventProp;
            private SerializedProperty callProp;
            private SerializedProperty CallProp
            {
                get
                {
                    if (callProp != null)
                        return callProp;
                    callProp = unityEventProp
                        .FindPropertyRelative("m_PersistentCalls")
                        .FindPropertyRelative("m_Calls")
                        .GetArrayElementAtIndex(index);
                    return callProp;
                }
            }
            private SerializedProperty argsProp;
            private SerializedProperty ArgsProp
            {
                get
                {
                    if (argsProp != null)
                        return argsProp;
                    argsProp = CallProp.FindPropertyRelative("m_Arguments");
                    return argsProp;
                }
            }
            private int index;

            public Object Target => unityEvent.GetPersistentTarget(index);
            public string MethodName => unityEvent.GetPersistentMethodName(index);
            public PersistentListenerMode ListenerMode
                => (PersistentListenerMode)CallProp.FindPropertyRelative("m_Mode").intValue;
            public Object ObjectArgument => ArgsProp.FindPropertyRelative("m_ObjectArgument").objectReferenceValue;
            public float FloatArgument => ArgsProp.FindPropertyRelative("m_FloatArgument").floatValue;
            public int IntArgument => ArgsProp.FindPropertyRelative("m_IntArgument").intValue;
            public string StringArgument => ArgsProp.FindPropertyRelative("m_StringArgument").stringValue;
            public bool BoolArgument => ArgsProp.FindPropertyRelative("m_BoolArgument").boolValue;
            public UnityEventCallState CallState
                => (UnityEventCallState)CallProp.FindPropertyRelative("m_CallState").intValue;

            public PersistentEventListenerWrapper(
                UnityEventBase unityEvent,
                SerializedProperty unityEventProp,
                int index)
            {
                this.unityEvent = unityEvent;
                this.index = index;
                this.unityEventProp = unityEventProp;
                callProp = null;
                argsProp = null;
            }
        }

        public static IEnumerable<PersistentEventListenerWrapper> EnumeratePersistentEventListeners(
            UnityEventBase unityEvent,
            SerializedProperty unityEventProperty)
        {
            for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
                yield return new PersistentEventListenerWrapper(unityEvent, unityEventProperty, i);
        }

        public static void ConditionalButton<T>(GUIContent buttonContent, IEnumerable<T> targets, System.Action<IEnumerable<T>> onButtonClick)
        {
            if (targets.Any() && GUILayout.Button(buttonContent))
                onButtonClick(targets);
        }

        public static IEnumerable<T> EmptyIfNull<T>(IEnumerable<T> enumerable)
        {
            if (enumerable != null)
                foreach (T value in enumerable)
                    yield return value;
        }

        /// <summary>
        /// The resulting value can be used for localPosition of the given transform.
        /// In other words, the position is relative to the parent of the given transform.
        /// </summary>
        public static Vector3 WorldToLocalPosition(Transform transform, Vector3 worldPosition)
        {
            return transform.parent == null
                ? worldPosition
                : transform.parent.InverseTransformPoint(worldPosition);
        }

        /// <summary>
        /// The resulting value can be used for localRotation of the given transform.
        /// In other words, the rotation is relative to the parent of the given transform.
        /// </summary>
        public static Quaternion WorldToLocalRotation(Transform transform, Quaternion worldRotation)
        {
            return transform.parent == null
                ? worldRotation
                : Quaternion.Inverse(transform.parent.rotation) * worldRotation;
            // Here's a mental model for this operation:
            // The world rotation of a transform could be expressed like this:
            // parent.worldRotation * this.localRotation == this.worldRotation;
            // Where * is saying "rotate by the left side, then rotate by the right side", that's how Unity works.
            // Now we can transform this equation by first rotating both sides with the inverse of parent.worldRotation:
            // Inverse(parent.worldRotation) * parent.worldRotation * this.localRotation == Inverse(parent.worldRotation) * this.worldRotation;
            // Rotating by the inverse and then rotating by what was inverted cancels out, leaving us with this:
            // this.localRotation == Inverse(parent.worldRotation) * this.worldRotation;
            // And there we go, localRotation is isolated on one side of the equation, so now we know how to go from world to local rotation.
            // Do note that order of operations matters, since with Quaternions foo * bar != bar * foo.
            // Similarly, foo * Inverse(foo) * bar == bar, however foo * bar * Inverse(foo) != bar.
        }
    }
}
