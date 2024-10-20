using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor.Build;
using UdonSharp;
using UdonSharpEditor;
using VRC.Udon;
using System.Diagnostics;
using System.Reflection;

namespace JanSharp
{
    [InitializeOnLoad]
    [DefaultExecutionOrder(-1000)]
    public static class OnBuildUtil
    {
        private static Dictionary<Type, OnBuildCallbackData> registeredTypes;
        private static List<OrderedOnBuildCallbackData> typesToLookForList;
        private static HashSet<Type> typesToSearchForCache;
        private static Dictionary<Type, List<OnBuildCallbackData>> matchingDataInBaseTypesCache;

        static OnBuildUtil()
        {
            registeredTypes = new Dictionary<Type, OnBuildCallbackData>();
            typesToLookForList = new List<OrderedOnBuildCallbackData>();
            typesToSearchForCache = null;
            matchingDataInBaseTypesCache = new Dictionary<Type, List<OnBuildCallbackData>>();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged; // Ikd if this is needed. Probably not?
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange data)
        {
            if (data == PlayModeStateChange.ExitingEditMode)
                RunOnBuild();
        }

        public static void RegisterType<T>(Func<T, bool> callback, int order = 0) where T : Component
        {
            Type type = typeof(T);
            if (registeredTypes.TryGetValue(type, out OnBuildCallbackData data))
            {
                if (data.allOrders.Contains(order))
                {
                    UnityEngine.Debug.LogError($"Attempt to register the same Component type with the same order twice (type: {type.Name}, order: {order}).");
                    return;
                }
                else
                    data.allOrders.Add(order);
            }
            else
            {
                data = new OnBuildCallbackData(type, new HashSet<int>() { order });
                registeredTypes.Add(type, data);
            }
            typesToLookForList.Add(new OrderedOnBuildCallbackData(data, order, callback.Method, callback.Target));
        }

        private static void FigureOutWhatTypesToReallySearchFor()
        {
            if (typesToSearchForCache != null)
                return;
            typesToSearchForCache = new HashSet<Type>();

            Dictionary<Type, List<Type>> inheriting = new Dictionary<Type, List<Type>>();

            // All this really does is take registeredTypes.Keys and copy it to typesToSearchForCache while
            // removing any type where one of its base types is another registered type. Effectively
            // deduplicating search results preemptively.
            foreach (OrderedOnBuildCallbackData data in typesToLookForList)
            {
                Type registeredType = data.data.type;
                if (inheriting.Remove(registeredType, out List<Type> typesToRemove))
                    foreach (Type toRemove in typesToRemove)
                        typesToSearchForCache.Remove(toRemove);
                Type currentType = registeredType;
                while (currentType != typeof(Component))
                {
                    currentType = currentType.BaseType;
                    if (typesToSearchForCache.Contains(currentType))
                        goto doubleContinue;
                    if (!inheriting.TryGetValue(currentType, out typesToRemove))
                    {
                        typesToRemove = new List<Type>();
                        inheriting.Add(currentType, typesToRemove);
                    }
                    typesToRemove.Add(registeredType);
                }
                typesToSearchForCache.Add(registeredType);
            doubleContinue:
                continue;
            }
        }

        private static IEnumerable<OnBuildCallbackData> GetMatchingDataForAllBaseTypes(Type mainType)
        {
            if (matchingDataInBaseTypesCache.TryGetValue(mainType, out List<OnBuildCallbackData> cached))
                return cached;
            List<OnBuildCallbackData> listOfData = new List<OnBuildCallbackData>();
            Type currentType = mainType;
            do
            {
                if (registeredTypes.TryGetValue(currentType, out OnBuildCallbackData data))
                    listOfData.Add(data);
                currentType = currentType.BaseType;
            }
            while (currentType != typeof(Component));
            // Yes do add empty lists to the cache, though with the current implementation for
            // FigureOutWhatTypesToReallySearchFor it actually should never be empty.
            matchingDataInBaseTypesCache.Add(mainType, listOfData);
            return listOfData;
        }

        private static void TryAddFoundComponentToOnBuildCallbackData(Component component)
        {
            foreach (OnBuildCallbackData data in GetMatchingDataForAllBaseTypes(component.GetType()))
                data.components.Add(component);
        }

        [MenuItem("Tools/JanSharp/Run All OnBuild handlers", priority = 10005)]
        public static bool RunOnBuild()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (OnBuildCallbackData data in registeredTypes.Values)
                data.components.Clear();

            FigureOutWhatTypesToReallySearchFor();

            foreach (Type type in typesToSearchForCache)
            {
                UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsByType(
                    type,
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.InstanceID);
                foreach (UnityEngine.Object obj in objects)
                    TryAddFoundComponentToOnBuildCallbackData((Component)obj);
            }

            foreach (OrderedOnBuildCallbackData orderedData in typesToLookForList.OrderBy(d => d.order))
                foreach (Component component in orderedData.data.components)
                    if (!(bool)orderedData.callbackInfo.Invoke(orderedData.callbackInstance, new[] { component }))
                    {
                        UnityEngine.Debug.LogError($"OnBuild handlers aborted when running the handler for '{component.GetType().Name}' on '{component.name}'.", component);
                        return false;
                    }

            sw.Stop();
            UnityEngine.Debug.Log($"OnBuild handlers: {sw.Elapsed}.");
            return true;
        }

        private struct OrderedOnBuildCallbackData
        {
            public OnBuildCallbackData data;
            public int order;
            public MethodInfo callbackInfo;
            public object callbackInstance;

            public OrderedOnBuildCallbackData(OnBuildCallbackData data, int order, MethodInfo callbackInfo, object callbackInstance)
            {
                this.data = data;
                this.order = order;
                this.callbackInfo = callbackInfo;
                this.callbackInstance = callbackInstance;
            }
        }

        private class OnBuildCallbackData
        {
            public Type type;
            public List<Component> components;
            public HashSet<int> allOrders;

            public OnBuildCallbackData(Type type, HashSet<int> allOrders)
            {
                this.type = type;
                this.components = new List<Component>();
                this.allOrders = allOrders;
            }
        }
    }

    ///cSpell:ignore IVRCSDK, VRCSDK

    public class VRCOnBuild : IVRCSDKBuildRequestedCallback
    {
        int IOrderedCallback.callbackOrder => 0;

        bool IVRCSDKBuildRequestedCallback.OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType == VRCSDKRequestedBuildType.Avatar)
                return true;
            return OnBuildUtil.RunOnBuild();
        }
    }
}
