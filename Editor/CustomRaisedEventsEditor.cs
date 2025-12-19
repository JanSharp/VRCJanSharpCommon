using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace JanSharp.Internal
{
    [InitializeOnLoad]
    [DefaultExecutionOrder(-1000)]
    public static class CustomRaisedEvents
    {
        private static bool didRegisterCoreHandlers = false;
        private static List<RegisteredData> allRegisteredData = new();
        private static Dictionary<System.Type, RegisteredData> registeredDataByCustomRaisedEventAttributeType = new();
        private static Dictionary<System.Type, List<(RegisteredData data, int eventTypeValue, int order, int defaultExecutionOrder)>> ubTypeCache = new();
        private const BindingFlags PrivateAndPublicFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private static bool hasValidationErrors = false;

        static CustomRaisedEvents()
        {
            hasValidationErrors = false;
            didRegisterCoreHandlers = false;
            ubTypeCache.Clear();
            ProcessAllUBTypes();
        }

        private static void ProcessAllUBTypes(bool validateOnly = false)
        {
            foreach (System.Type ubType in OnAssemblyLoadUtil.AllUdonSharpBehaviourTypes)
            {
                CustomRaisedEventsDispatcherAttribute dispatcherAttr = ubType.GetCustomAttribute<CustomRaisedEventsDispatcherAttribute>();
                if (dispatcherAttr == null)
                    continue;
                if (!ValidateCustomRaisedEventAttributeType(ubType, dispatcherAttr)
                    || !ValidateCustomEventEnumType(ubType, dispatcherAttr, out List<int> eventTypeValues)
                    || !ValidateListenerFields(ubType, dispatcherAttr))
                {
                    hasValidationErrors = true;
                    continue;
                }
                if (!validateOnly)
                    RegisterDispatcher(ubType, dispatcherAttr, eventTypeValues);
            }
        }

        private static void RegisterCoreHandlers()
        {
            if (didRegisterCoreHandlers)
                return;
            didRegisterCoreHandlers = true;
            OnBuildUtil.RegisterAction(PreOnBuild, order: -102);
            OnBuildUtil.RegisterType<UdonSharpBehaviour>(EventListenersOnBuild, order: -100);
            OnBuildUtil.RegisterAction(PostOnBuild, order: -99);
        }

        private static bool ValidateCustomRaisedEventAttributeType(System.Type ubType, CustomRaisedEventsDispatcherAttribute attr)
        {
            if (attr.CustomRaisedEventAttributeType == null)
            {
                Debug.LogError($"[JanSharpCommon] The {nameof(CustomRaisedEventsDispatcherAttribute)} for the class "
                    + $"{ubType.Name} is missing the attribute type argument.");
                return false;
            }
            if (!EditorUtil.DerivesFrom(attr.CustomRaisedEventAttributeType, typeof(CustomRaisedEventBaseAttribute)))
            {
                Debug.LogError($"[JanSharpCommon] The {nameof(CustomRaisedEventsDispatcherAttribute)} for the class "
                    + $"{ubType.Name} is referencing {attr.CustomRaisedEventAttributeType.Name} as its attribute type argument "
                    + $"which is not deriving from {nameof(CustomRaisedEventBaseAttribute)}.");
                return false;
            }
            var attributeUsageAttr = attr.CustomRaisedEventAttributeType.GetCustomAttribute<System.AttributeUsageAttribute>();
            if (attributeUsageAttr == null)
            {
                Debug.LogError($"[JanSharpCommon] The {nameof(CustomRaisedEventsDispatcherAttribute)} for the class "
                    + $"{ubType.Name} is referencing {attr.CustomRaisedEventAttributeType.Name} as its attribute type argument "
                    + $"is missing the {nameof(System.AttributeUsageAttribute)}.");
                return false;
            }
            if (attributeUsageAttr.ValidOn != System.AttributeTargets.Method)
            {
                Debug.LogError($"[JanSharpCommon] The {nameof(CustomRaisedEventsDispatcherAttribute)} for the class "
                    + $"{ubType.Name} is referencing {attr.CustomRaisedEventAttributeType.Name} as its attribute type argument "
                    + $"where the {nameof(System.AttributeUsageAttribute)}'s ValidOn AttributeTargets is not equal to AttributeTargets.Method.");
                return false;
            }
            return true;
        }

        private static bool ValidateCustomEventEnumType(System.Type ubType, CustomRaisedEventsDispatcherAttribute attr, out List<int> eventTypeValues)
        {
            eventTypeValues = null;
            if (attr.CustomEventEnumType == null)
            {
                Debug.LogError($"[JanSharpCommon] The {nameof(CustomRaisedEventsDispatcherAttribute)} for the class "
                    + $"{ubType.Name} is missing the enum type argument.");
                return false;
            }
            if (!EditorUtil.DerivesFrom(attr.CustomEventEnumType, typeof(System.Enum)))
            {
                Debug.LogError($"[JanSharpCommon] The {nameof(CustomRaisedEventsDispatcherAttribute)} for the class "
                    + $"{ubType.Name} is referencing the type {attr.CustomEventEnumType.Name} as its enum type argument "
                    + $"which is not an enum.");
                return false;
            }
            if (System.Enum.GetUnderlyingType(attr.CustomEventEnumType) != typeof(int))
            {
                Debug.LogError($"[JanSharpCommon] The {nameof(CustomRaisedEventsDispatcherAttribute)} for the class "
                    + $"{ubType.Name} is referencing the type {attr.CustomEventEnumType.Name} as its enum type argument "
                    + $"which is using a non int (System.Int32) as its underlying type.");
                return false;
            }
            eventTypeValues = System.Enum.GetValues(attr.CustomEventEnumType).Cast<int>().ToList();
            if (eventTypeValues.Count != eventTypeValues.Distinct().Count())
            {
                Debug.LogError($"[JanSharpCommon] The {nameof(CustomRaisedEventsDispatcherAttribute)} for the class "
                    + $"{ubType.Name} is referencing the type {attr.CustomEventEnumType.Name} as its enum type argument "
                    + $"which does not have unique values for each of its enum fields.");
                return false;
            }
            return true;
        }

        private static bool ValidateListenerFields(System.Type ubType, CustomRaisedEventsDispatcherAttribute attr)
        {
            bool result = true;
            foreach (string name in System.Enum.GetNames(attr.CustomEventEnumType))
            {
                FieldInfo fieldInfo = ubType.GetField(GetListenersFieldName(name), PrivateAndPublicFlags);
                if (fieldInfo == null)
                {
                    Debug.LogError($"[JanSharpCommon] Missing {GetListenersFieldName(name)} field for the class {ubType.Name}, "
                        + $"this is required for the {nameof(CustomRaisedEventsDispatcherAttribute)} to work. "
                        + $"The filed must have the type UdonSharpBehaviour[] and be serialized by Unity "
                        + $"(either be public or have the {nameof(SerializeField)} attribute).");
                    result = false;
                    continue;
                }
                if (fieldInfo.FieldType != typeof(UdonSharpBehaviour[]))
                {
                    Debug.LogError($"[JanSharpCommon] The field {GetListenersFieldName(name)} for the class {ubType.Name} "
                        + $"must have the type UdonSharpBehaviour[],"
                        + $"this is required for the {nameof(CustomRaisedEventsDispatcherAttribute)} to work.");
                    result = false;
                }
                if (!EditorUtil.IsSerializedField(fieldInfo))
                {
                    Debug.LogError($"[JanSharpCommon] The field {GetListenersFieldName(name)} for the class {ubType.Name} "
                        + $"must be serialized by Unity (either be public or have the {nameof(SerializeField)} attribute),"
                        + $"this is required for the {nameof(CustomRaisedEventsDispatcherAttribute)} to work.");
                    result = false;
                }
            }
            return result;
        }

        private static string GetListenersFieldName(string eventTypeName)
        {
            return char.ToLower(eventTypeName[0]) + eventTypeName[1..] + "Listeners";
        }

        private static void RegisterDispatcher(System.Type ubType, CustomRaisedEventsDispatcherAttribute dispatcherAttr, List<int> eventTypeValues)
        {
            RegisterCoreHandlers();
            List<(string name, int value)> eventTypeNameValuePairs = System.Enum.GetNames(dispatcherAttr.CustomEventEnumType)
                .Select((name, i) => (name, eventTypeValues[i]))
                .ToList();
            RegisteredData registeredData = new RegisteredData()
            {
                ubType = ubType,
                eventTypeEnumType = dispatcherAttr.CustomEventEnumType,
                findListenersInChildrenOnly = dispatcherAttr.FindListenersInChildrenOnly,
                eventTypes = eventTypeNameValuePairs
                    .Select(e => (GetListenersFieldName(e.name), e.value))
                    .ToList(),
                eventTypeToNameLut = eventTypeNameValuePairs.ToDictionary(e => e.value, e => e.name),
            };
            allRegisteredData.Add(registeredData);
            registeredDataByCustomRaisedEventAttributeType.Add(dispatcherAttr.CustomRaisedEventAttributeType, registeredData);
            OnBuildUtil.RegisterTypeCumulative(ubType, dispatchers =>
            {
                registeredData.dispatchers = dispatchers.Cast<UdonSharpBehaviour>().ToArray();
                return true;
            }, order: -101);
        }

        private class RegisteredData
        {
            public System.Type ubType;
            public System.Type eventTypeEnumType;
            public bool findListenersInChildrenOnly;
            public List<(string name, int value)> eventTypes;
            public Dictionary<int, string> eventTypeToNameLut;
            public UdonSharpBehaviour[] dispatchers;
            public Dictionary<int, List<(UdonSharpBehaviour ub, int order, int defaultExecutionOrder)>> allListeners = new();
        }

        private static bool PreOnBuild()
        {
            if (hasValidationErrors)
            {
                ProcessAllUBTypes(validateOnly: true);
                return false;
            }
            foreach (RegisteredData data in allRegisteredData)
                data.allListeners.Clear();
            return true;
        }

        private static List<(RegisteredData data, int eventTypeValue, int order, int defaultExecutionOrder)> GetTypeCache(UdonSharpBehaviour ub, System.Type ubType)
        {
            if (ubTypeCache.TryGetValue(ubType, out var cached))
                return cached;
            cached = new();
            int defaultExecutionOrder = ubType.GetCustomAttribute<DefaultExecutionOrder>()?.order ?? 0;

            foreach (MethodInfo method in ubType.GetMethods(PrivateAndPublicFlags))
            {
                foreach (CustomRaisedEventBaseAttribute attr in method.GetCustomAttributes<CustomRaisedEventBaseAttribute>())
                {
                    if (attr == null
                        || !registeredDataByCustomRaisedEventAttributeType.TryGetValue(attr.GetType(), out RegisteredData data))
                        continue;
                    if (!data.eventTypeToNameLut.TryGetValue(attr.CustomEventTypeEnumValue, out string eventName))
                    {
                        Debug.LogError($"[JanSharpCommon] The method name {method.Name} for the {ubType.Name} script "
                            + $"has an invalid enum value of {attr.CustomEventTypeEnumValue} for its {attr.GetType().Name}.", ub);
                        return null;
                    }
                    if (method.Name != eventName)
                    {
                        Debug.LogError($"[JanSharpCommon] The method name {method.Name} does not match the expected "
                            + $"custom raised event name {eventName} for the {ubType.Name} script.", ub);
                        return null;
                    }
                    if (!method.IsPublic)
                    {
                        Debug.LogError($"[JanSharpCommon] The method {ubType.Name}.{method.Name} is marked "
                            + $"as a custom raised event, however event methods must be public.", ub);
                        return null;
                    }
                    cached.Add((data, attr.CustomEventTypeEnumValue, attr.Order, defaultExecutionOrder));
                }
            }

            ubTypeCache.Add(ubType, cached);
            return cached;
        }

        private static bool EventListenersOnBuild(UdonSharpBehaviour ub)
        {
            System.Type ubType = ub.GetType();
            var cached = GetTypeCache(ub, ubType);
            if (cached == null)
                return false;

            foreach (var listener in cached)
            {
                if (!listener.data.allListeners.TryGetValue(listener.eventTypeValue, out var listeners))
                {
                    listeners = new();
                    listener.data.allListeners.Add(listener.eventTypeValue, listeners);
                }
                listeners.Add((ub, listener.order, listener.defaultExecutionOrder));
            }

            return true;
        }

        private static bool PostOnBuild()
        {
            foreach (RegisteredData data in allRegisteredData)
                if (data.findListenersInChildrenOnly)
                    AssignAnyChildren(data);
                else
                    AssignFromAnywhereInScene(data);
            return true;
        }

        private static void AssignFromAnywhereInScene(RegisteredData data)
        {
            if (data.dispatchers.Length == 0)
                return;
            SerializedObject dispatchersSo = new SerializedObject(data.dispatchers);
            foreach (var eventType in data.eventTypes)
            {
                SerializedProperty listenersProperty = dispatchersSo.FindProperty(eventType.name);
                if (!data.allListeners.TryGetValue(eventType.value, out var listeners))
                {
                    listenersProperty.arraySize = 0;
                    continue;
                }
                EditorUtil.SetArrayProperty(listenersProperty,
                    listeners.OrderBy(v => v.order).ThenBy(v => v.defaultExecutionOrder).ToList(),
                    (p, v) => p.objectReferenceValue = v.ub);
            }
            dispatchersSo.ApplyModifiedProperties();
        }

        private static void AssignAnyChildren(RegisteredData data)
        {
            foreach (UdonSharpBehaviour dispatcher in data.dispatchers)
            {
                SerializedObject dispatchersSo = new SerializedObject(dispatcher);
                Transform dispatcherTransform = dispatcher.transform;
                foreach (var eventType in data.eventTypes)
                {
                    SerializedProperty listenersProperty = dispatchersSo.FindProperty(eventType.name);
                    if (!data.allListeners.TryGetValue(eventType.value, out var listeners))
                    {
                        listenersProperty.arraySize = 0;
                        continue;
                    }
                    EditorUtil.SetArrayProperty(listenersProperty,
                        listeners
                            .Where(v => EditorUtil.IsChild(dispatcherTransform, v.ub.transform))
                            .OrderBy(v => v.order)
                            .ThenBy(v => v.defaultExecutionOrder)
                            .ToList(),
                        (p, v) => p.objectReferenceValue = v.ub);
                }
                dispatchersSo.ApplyModifiedProperties();
            }
        }
    }
}
