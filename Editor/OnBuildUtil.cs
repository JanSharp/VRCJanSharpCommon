using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor.Build;
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
        private static bool rerunDueToScriptInstantiation = false;
        private static int runCounter = 1;
        private const int MaxRerunRecursion = 20;

        static OnBuildUtil()
        {
            registeredTypes = new Dictionary<Type, OnBuildCallbackData>();
            typesToLookForList = new List<OrderedOnBuildCallbackData>();
            typesToSearchForCache = null;
            matchingDataInBaseTypesCache = new Dictionary<Type, List<OnBuildCallbackData>>();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged; // Idk if this is needed. Probably not?
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange data)
        {
            if (data == PlayModeStateChange.ExitingEditMode)
                if (!RunOnBuild(showDialogOnFailure: true, useSceneViewNotification: true, abortIfRerunsHappened: true))
                    EditorApplication.isPlaying = false;
        }

        public static void RegisterType<T>(
            Func<T, bool> callback,
            int order = 0,
            bool includeEditorOnly = false)
            where T : Component
        {
            RegisterTypeInternal(typeof(T), callback, order, includeEditorOnly, usesCustomCallbackParamType: false, null);
        }

        public static void RegisterTypeCumulative<T>(
            Func<IEnumerable<T>, bool> callback,
            int order = 0,
            bool includeEditorOnly = false)
            where T : Component
        {
            RegisterTypeInternal(typeof(T), callback, order, includeEditorOnly, usesCustomCallbackParamType: true, c => c.Cast<T>());
        }

        public static void RegisterType(
            Type type,
            Func<Component, bool> callback,
            int order = 0,
            bool includeEditorOnly = false)
        {
            if (!EditorUtil.DerivesFrom(type, typeof(Component)))
                throw new ArgumentException($"The given type to register must derive from the Component class.");
            RegisterTypeInternal(type, callback, order, includeEditorOnly, usesCustomCallbackParamType: false, null);
        }

        public static void RegisterTypeCumulative(
            Type type,
            Func<ReadOnlyCollection<Component>, bool> callback,
            int order = 0,
            bool includeEditorOnly = false)
        {
            if (!EditorUtil.DerivesFrom(type, typeof(Component)))
                throw new ArgumentException($"The given type to register must derive from the Component class.");
            RegisterTypeInternal(type, callback, order, includeEditorOnly, usesCustomCallbackParamType: true, c => c.AsReadOnly());
        }

        public static void RegisterAction(Func<bool> callback, int order = 0)
        {
            typesToLookForList.Add(new OrderedOnBuildCallbackData(null, order, false, callback.Method, callback.Target, false, null));
        }

        private static void RegisterTypeInternal(
            Type type,
            Delegate callback,
            int order,
            bool includeEditorOnly,
            bool usesCustomCallbackParamType,
            Func<List<Component>, object> toCorrectlyTypedCallbackParamType)
        {
            if (registeredTypes.TryGetValue(type, out OnBuildCallbackData data))
            {
                if (data.allOrders.Contains(order))
                {
                    UnityEngine.Debug.LogError($"[JanSharpCommon] Attempt to register the same Component type with the same order twice (type: {type.Name}, order: {order}).");
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
            typesToLookForList.Add(new OrderedOnBuildCallbackData(
                data,
                order,
                includeEditorOnly,
                callback.Method,
                callback.Target,
                usesCustomCallbackParamType,
                toCorrectlyTypedCallbackParamType));
        }

        /// <summary>
        /// <para>When instantiating UdonSharpBehaviours (and probably also UdonBehaviours) inside of an
        /// OnBuild handler, call this in order for all OnBuild handlers to be restarted from the
        /// beginning.</para>
        /// <para>Additionally even when all OnBuild handlers succeed, if scripts were instantiated entering
        /// play mode or uploading is going to get aborted because VRChat's sdk must do some processing on the
        /// newly instantiated script instances.</para>
        /// </summary>
        public static void MarkForRerunDueToScriptInstantiation()
        {
            rerunDueToScriptInstantiation = true;
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
            foreach (OrderedOnBuildCallbackData orderedData in typesToLookForList)
            {
                if (orderedData.data == null)
                    continue;
                Type registeredType = orderedData.data.type;
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
            bool editorOnly = EditorUtil.IsEditorOnly(component);
            foreach (OnBuildCallbackData data in GetMatchingDataForAllBaseTypes(component.GetType()))
                data.components.Add((component, editorOnly));
        }

        [MenuItem("Tools/JanSharp/Run All OnBuild handlers", priority = 10005)]
        public static void RunOnBuildMenuItem()
        {
            RunOnBuild(showDialogOnFailure: true, useSceneViewNotification: false, abortIfRerunsHappened: false);
        }

        private static bool RunOnBuildIteration()
        {
            foreach (OnBuildCallbackData data in registeredTypes.Values)
                data.components.Clear();

            FigureOutWhatTypesToReallySearchFor();

            using (EditorUtil.BatchedEditorOnlyChecks())
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
            {
                if (!orderedData.InvokeCallback())
                    return false;
                if (rerunDueToScriptInstantiation)
                    return false; // Return value does not matter here.
            }
            return true;
        }

        public static bool RunOnBuild(bool showDialogOnFailure, bool useSceneViewNotification, bool abortIfRerunsHappened)
        {
            void ShowNotification(string msg)
            {
                if (!useSceneViewNotification || !ShowSceneViewNotification(msg))
                    EditorUtility.DisplayDialog("JanSharpCommon OnBuild", msg, "Ok");
            }

            rerunDueToScriptInstantiation = false;
            runCounter = 1;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool success;
            bool reachedMaxRerunRecursion = false;
            while (true)
            {
                success = RunOnBuildIteration();
                if (!rerunDueToScriptInstantiation)
                    break;
                if (runCounter == MaxRerunRecursion)
                {
                    reachedMaxRerunRecursion = true;
                    break;
                }
                rerunDueToScriptInstantiation = false;
                runCounter++;
            }
            sw.Stop();

            if (success && runCounter != 1 && abortIfRerunsHappened)
            {
                UnityEngine.Debug.LogError("Udon scripts were instantiated during the OnBuild process, VRChat "
                    + "may need to do some setup for those, I don't know, either way please try again.");
                ShowNotification("Scripts were instantiated during OnBuild, please go again.");
                return false;
            }

            if (success)
            {
                UnityEngine.Debug.Log($"[JanSharpCommon] OnBuild handlers took: {sw.Elapsed}."
                    + (runCounter == 1 ? "" : $" (Had tu run {runCounter} times.)"));
                return true;
            }

            if (!showDialogOnFailure)
                return false;

            ShowNotification(reachedMaxRerunRecursion
                ? $"OnBuild handlers reran {MaxRerunRecursion} times due to scripts "
                    + $"being instantiated mid run, could be recursive, aborting."
                : $"OnBuild handlers failed, check the Console to review errors.");
            return false;
        }

        private static bool ShowSceneViewNotification(string notification)
        {
            if (SceneView.lastActiveSceneView == null)
                return false;
            SceneView.lastActiveSceneView.Focus();
            SceneView.lastActiveSceneView.ShowNotification(new GUIContent(notification));
            return true;
        }

        private struct OrderedOnBuildCallbackData
        {
            public OnBuildCallbackData data;
            public int order;
            public bool includeEditorOnly;
            public MethodInfo callbackInfo;
            public object callbackInstance;
            public bool usesCustomCallbackParamType;
            public Func<List<Component>, object> toCorrectlyTypedCallbackParamType;

            public readonly bool IsAction => data == null;

            public OrderedOnBuildCallbackData(
                OnBuildCallbackData data,
                int order,
                bool includeEditorOnly,
                MethodInfo callbackInfo,
                object callbackInstance,
                bool usesCustomCallbackParamType,
                Func<List<Component>, object> toCorrectlyTypedCallbackParamType)
            {
                this.data = data;
                this.order = order;
                this.includeEditorOnly = includeEditorOnly;
                this.callbackInfo = callbackInfo;
                this.callbackInstance = callbackInstance;
                this.usesCustomCallbackParamType = usesCustomCallbackParamType;
                this.toCorrectlyTypedCallbackParamType = toCorrectlyTypedCallbackParamType;
            }

            public readonly bool InvokeCallback()
            {
                return IsAction
                    ? InvokeActionCallback()
                    : usesCustomCallbackParamType
                        ? InvokeCallbackWithCustomParamType()
                        : InvokeCallbackForeach();
            }

            private readonly bool InvokeCallbackForeach()
            {
                foreach (Component component in data.GetComponents(includeEditorOnly))
                    if (!(bool)callbackInfo.Invoke(callbackInstance, new[] { component }))
                    {
                        UnityEngine.Debug.LogError($"[JanSharpCommon] OnBuild handlers aborted when running the handler for '{data.type.Name}' on '{component.name}'.", component);
                        return false;
                    }
                return true;
            }

            private readonly bool InvokeCallbackWithCustomParamType()
            {
                if (!(bool)callbackInfo.Invoke(
                    callbackInstance,
                    new[] { toCorrectlyTypedCallbackParamType(data.GetComponents(includeEditorOnly).ToList()) }))
                {
                    UnityEngine.Debug.LogError($"[JanSharpCommon] OnBuild handlers aborted when running the handler for '{data.type.Name}'.");
                    return false;
                }
                return true;
            }

            private readonly bool InvokeActionCallback()
            {
                if (!(bool)callbackInfo.Invoke(callbackInstance, new object[0]))
                {
                    UnityEngine.Debug.LogError($"[JanSharpCommon] OnBuild handlers aborted when running the action {callbackInfo.Name}.");
                    return false;
                }
                return true;
            }
        }

        private class OnBuildCallbackData
        {
            public Type type;
            public List<(Component component, bool editorOnly)> components;
            public HashSet<int> allOrders;

            public OnBuildCallbackData(Type type, HashSet<int> allOrders)
            {
                this.type = type;
                this.components = new List<(Component component, bool editorOnly)>();
                this.allOrders = allOrders;
            }

            public IEnumerable<Component> GetComponents(bool includeEditorOnly)
            {
                IEnumerable<(Component component, bool editorOnly)> result = components;
                if (!includeEditorOnly)
                    result = result.Where(c => !c.editorOnly);
                return components.Select(c => c.component);
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
            return OnBuildUtil.RunOnBuild(showDialogOnFailure: false, useSceneViewNotification: false, abortIfRerunsHappened: true);
        }
    }
}
