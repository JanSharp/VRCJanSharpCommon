#if UNITY_EDITOR
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
        private static Dictionary<Type, OnBuildCallbackData> typesToLookFor;
        private static List<OrderedOnBuildCallbackData> typesToLookForList;

        static OnBuildUtil()
        {
            typesToLookFor = new Dictionary<Type, OnBuildCallbackData>();
            typesToLookForList = new List<OrderedOnBuildCallbackData>();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange data)
        {
            if (data == PlayModeStateChange.ExitingEditMode)
                RunOnBuild();
        }

        public static void RegisterType<T>(Func<T, bool> callback, int order = 0) where T : UdonSharpBehaviour
        {
            Type type = typeof(T);
            OnBuildCallbackData data;
            if (typesToLookFor.TryGetValue(type, out data))
            {
                if (data.allOrders.Contains(order))
                {
                    UnityEngine.Debug.LogError($"Attempt to register the same UdonSharpBehaviour type with the same order twice (type: {type.Name}, order: {order}).");
                    return;
                }
                else
                    data.allOrders.Add(order);
            }
            else
            {
                data = new OnBuildCallbackData(type, new HashSet<int>() { order });
                typesToLookFor.Add(type, data);
            }
            typesToLookForList.Add(new OrderedOnBuildCallbackData(data, order, callback.Method, callback.Target));
        }

        [MenuItem("Tools/Run all OnBuild handlers")]
        public static bool RunOnBuild()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (OnBuildCallbackData data in typesToLookFor.Values)
                data.behaviours.Clear();

            foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (UdonBehaviour udonBehaviour in obj.GetComponentsInChildren<UdonBehaviour>())
                    if (UdonSharpEditorUtility.IsUdonSharpBehaviour(udonBehaviour))
                    {
                        UdonSharpBehaviour behaviour = UdonSharpEditorUtility.GetProxyBehaviour(udonBehaviour);
                        if (typesToLookFor.TryGetValue(behaviour.GetType(), out OnBuildCallbackData data))
                            data.behaviours.Add(behaviour);
                    }

            foreach (OrderedOnBuildCallbackData orderedData in typesToLookForList.OrderBy(d => d.order))
                foreach (UdonSharpBehaviour behaviour in orderedData.data.behaviours)
                    if (!(bool)orderedData.callbackInfo.Invoke(orderedData.callbackInstance, new[] { behaviour }))
                    {
                        UnityEngine.Debug.LogError($"OnBuild handlers aborted when running the handler for '{behaviour.GetType().Name}' on '{behaviour.name}'.", behaviour);
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
            public List<UdonSharpBehaviour> behaviours;
            public HashSet<int> allOrders;

            public OnBuildCallbackData(Type type, HashSet<int> allOrders)
            {
                this.type = type;
                this.behaviours = new List<UdonSharpBehaviour>();
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
#endif
