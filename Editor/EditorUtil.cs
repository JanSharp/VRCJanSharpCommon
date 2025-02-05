using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using VRC.Udon;

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
                    m_TargetAssemblyTypeName: UnityEngine.GameObject, UnityEngine
                    m_MethodName: SendCustomEvent
                    m_Mode: 5
                    m_Arguments:
                      m_ObjectArgument: {fileID: 0}
                      m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine
                      m_IntArgument: 0
                      m_FloatArgument: 0
                      m_StringArgument: OnValueChanged
                      m_BoolArgument: 0
                    m_CallState: 2
            */

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

            /// <summary>
            /// This may not be a sufficient or correct implementation to get/set the object reference
            /// argument, since alongside the m_Target property - which is accessed by this getter and
            /// setter - there is also the m_TargetAssemblyTypeName property, which may or may not be
            /// handled automatically. I do not know.
            /// </summary>
            public Object Target
            {
                get => CallProp.FindPropertyRelative("m_Target").objectReferenceValue;
                set => CallProp.FindPropertyRelative("m_Target").objectReferenceValue = value;
            }
            public string MethodName
            {
                get => CallProp.FindPropertyRelative("m_MethodName").stringValue;
                set => CallProp.FindPropertyRelative("m_MethodName").stringValue = value;
            }
            public PersistentListenerMode ListenerMode
            {
                get => (PersistentListenerMode)CallProp.FindPropertyRelative("m_Mode").intValue;
                set => CallProp.FindPropertyRelative("m_Mode").intValue = (int)value;
            }
            /// <summary>
            /// This may not be a sufficient or correct implementation to get/set the object reference
            /// argument, since alongside the ObjectArgument property - which is accessed by this getter and
            /// setter - there is also the ObjectArgumentAssemblyTypeName property, which may or may not be
            /// handled automatically. I do not know.
            /// </summary>
            public Object ObjectArgument
            {
                get => ArgsProp.FindPropertyRelative("m_ObjectArgument").objectReferenceValue;
                set => ArgsProp.FindPropertyRelative("m_ObjectArgument").objectReferenceValue = value;
            }
            public float FloatArgument
            {
                get => ArgsProp.FindPropertyRelative("m_FloatArgument").floatValue;
                set => ArgsProp.FindPropertyRelative("m_FloatArgument").floatValue = value;
            }
            public int IntArgument
            {
                get => ArgsProp.FindPropertyRelative("m_IntArgument").intValue;
                set => ArgsProp.FindPropertyRelative("m_IntArgument").intValue = value;
            }
            public string StringArgument
            {
                get => ArgsProp.FindPropertyRelative("m_StringArgument").stringValue;
                set => ArgsProp.FindPropertyRelative("m_StringArgument").stringValue = value;
            }
            public bool BoolArgument
            {
                get => ArgsProp.FindPropertyRelative("m_BoolArgument").boolValue;
                set => ArgsProp.FindPropertyRelative("m_BoolArgument").boolValue = value;
            }
            public UnityEventCallState CallState
            {
                get => (UnityEventCallState)CallProp.FindPropertyRelative("m_CallState").intValue;
                set => CallProp.FindPropertyRelative("m_CallState").intValue = (int)value;
            }

            public PersistentEventListenerWrapper(SerializedProperty unityEventProp, int index)
            {
                this.unityEventProp = unityEventProp;
                this.index = index;
                callProp = null;
                argsProp = null;
            }
        }

        public static PersistentEventListenerWrapper AddPersistentEventListener(SerializedProperty unityEventProp)
        {
            SerializedProperty callsProp = unityEventProp
                .FindPropertyRelative("m_PersistentCalls")
                .FindPropertyRelative("m_Calls");
            callsProp.arraySize++;
            return new PersistentEventListenerWrapper(unityEventProp, callsProp.arraySize - 1);
        }

        /// <summary>
        /// Make sure to call <see cref="SerializedObject.ApplyModifiedProperties"/> after calling this.
        /// </summary>
        /// <param name="unityEventProp"></param>
        public static void AddPersistentSendCustomEventListener(
            SerializedProperty unityEventProp,
            UdonBehaviour target,
            string customEventName)
        {
            var listener = AddPersistentEventListener(unityEventProp);
            listener.Target = target;
            listener.MethodName = nameof(UdonBehaviour.SendCustomEvent);
            listener.ListenerMode = PersistentListenerMode.String;
            listener.StringArgument = customEventName;
            listener.CallState = UnityEventCallState.RuntimeOnly;
        }

        /// <summary>
        /// Make sure to call <see cref="SerializedObject.ApplyModifiedProperties"/> after calling this.
        /// </summary>
        /// <param name="unityEventProp"></param>
        public static void AddPersistentInteractListener(
            SerializedProperty unityEventProp,
            UdonBehaviour target)
        {
            var listener = AddPersistentEventListener(unityEventProp);
            listener.Target = target;
            listener.MethodName = nameof(UdonBehaviour.Interact);
            listener.ListenerMode = PersistentListenerMode.Void;
            listener.CallState = UnityEventCallState.RuntimeOnly;
        }

        public static bool HasCustomEventListener(
            SerializedProperty unityEventProp,
            UdonBehaviour target,
            string customEventName)
        {
            return EditorUtil.EnumeratePersistentEventListeners(unityEventProp)
                .Any(l => l.Target is UdonBehaviour targetBehaviour
                    && targetBehaviour == target
                    && l.MethodName == nameof(UdonBehaviour.SendCustomEvent)
                    && l.StringArgument == customEventName);
        }

        public static IEnumerable<PersistentEventListenerWrapper> EnumeratePersistentEventListeners(SerializedProperty unityEventProperty)
        {
            SerializedProperty calls = unityEventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
            for (int i = 0; i < calls.arraySize; i++)
                yield return new PersistentEventListenerWrapper(unityEventProperty, i);
        }

        public static void ConditionalButton<T>(
            GUIContent buttonContent,
            IEnumerable<T> targets,
            System.Action<IEnumerable<T>> onButtonClick)
        {
            if (targets.Any() && GUILayout.Button(buttonContent))
                onButtonClick(targets);
        }

        public static void ConditionalRegisterCustomEventListenerButton<TTarget, TEventSource>(
            GUIContent buttonContent,
            IEnumerable<TTarget> targets,
            System.Func<TTarget, TEventSource> getEventSource,
            string unityEventSerializedPropertyPath,
            string customEventName
        )
            where TTarget : UdonSharpBehaviour
            where TEventSource : Object
        {
            EditorUtil.ConditionalButton(
                buttonContent,
                targets
                    .Select(t => (
                        source: getEventSource(t),
                        udonBehaviour: UdonSharpEditorUtility.GetBackingUdonBehaviour(t)
                    ))
                    .Where(d => d.source != null
                        && !HasCustomEventListener(new SerializedObject(d.source).FindProperty(unityEventSerializedPropertyPath),
                            d.udonBehaviour,
                            customEventName)
                    ),
                d => {
                    foreach (var target in d)
                    {
                        SerializedObject proxy = new SerializedObject(target.source);
                        AddPersistentSendCustomEventListener(
                            proxy.FindProperty(unityEventSerializedPropertyPath),
                            target.udonBehaviour,
                            customEventName);
                        proxy.ApplyModifiedProperties();
                    }
                }
            );
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

        public static bool DerivesFrom(System.Type typeToCheck, System.Type baseTypeToSearchFor)
        {
            while (typeToCheck != null)
            {
                if (typeToCheck == baseTypeToSearchFor)
                    return true;
                typeToCheck = typeToCheck.BaseType;
            }
            return false;
        }

        /// <summary>
        /// <para>https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html</para>
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool IsSerializedField(FieldInfo field)
        {
            return !field.IsStatic
                && !field.IsInitOnly
                && !field.IsDefined(typeof(System.NonSerializedAttribute), false)
                && (field.IsPublic || field.IsDefined(typeof(SerializeField), false));
        }

        /// <summary>
        /// <para>Use this specifically to also get private and protected fields from base types. If just
        /// public members are desired, use <see cref="BindingFlags.FlattenHierarchy"/>.</para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetFieldsIncludingBase(
            System.Type type,
            BindingFlags bindingAttr,
            System.Type stopAtType = null)
        {
            HashSet<string> visitedNames = new HashSet<string>();
            while (type != stopAtType)
            {
                foreach (FieldInfo field in type.GetFields(bindingAttr))
                {
                    if (visitedNames.Contains(field.Name))
                        continue;
                    visitedNames.Add(field.Name);
                    yield return field;
                }
                type = type.BaseType;
            }
        }

        /// <summary>
        /// <para>Use this specifically to also get a private or protected field from base types. If just
        /// public members are desired, use <see cref="BindingFlags.FlattenHierarchy"/>.</para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fieldName"></param>
        /// <param name="bindingAttr"></param>
        /// <param name="stopAtType"></param>
        /// <returns></returns>
        public static FieldInfo GetFieldIncludingBase(
            System.Type type,
            string fieldName,
            BindingFlags bindingAttr,
            System.Type stopAtType = null)
        {
            while (type != stopAtType)
            {
                FieldInfo fieldInfo = type.GetField(fieldName, bindingAttr);
                if (fieldInfo != null)
                    return fieldInfo;
                type = type.BaseType;
            }
            return null;
        }

        /// <summary>
        /// <para>Use this specifically to also get private and protected methods from base types. If just
        /// public members are desired, use <see cref="BindingFlags.FlattenHierarchy"/>.</para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMethodsIncludingBase(
            System.Type type,
            BindingFlags bindingAttr,
            System.Type stopAtType = null)
        {
            HashSet<string> visitedNames = new HashSet<string>();
            while (type != stopAtType)
            {
                foreach (MethodInfo method in type.GetMethods(bindingAttr))
                {
                    if (visitedNames.Contains(method.Name))
                        continue;
                    visitedNames.Add(method.Name);
                    yield return method;
                }
                type = type.BaseType;
            }
        }

        /// <summary>
        /// <para>Use this specifically to also get a private or protected method from base types. If just
        /// public members are desired, use <see cref="BindingFlags.FlattenHierarchy"/>.</para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="bindingAttr"></param>
        /// <param name="stopAtType"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodIncludingBase(
            System.Type type,
            string methodName,
            BindingFlags bindingAttr,
            System.Type stopAtType = null)
        {
            while (type != stopAtType)
            {
                MethodInfo methodInfo = type.GetMethod(methodName, bindingAttr);
                if (methodInfo != null)
                    return methodInfo;
                type = type.BaseType;
            }
            return null;
        }
    }
}
