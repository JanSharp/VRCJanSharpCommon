using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace JanSharp
{
    [InitializeOnLoad] // [DefaultExecutionOrder] has no effect on [InitializeOnLoad]
    public static class OnAssemblyLoadUtil
    {
        static OnAssemblyLoadUtil()
        {
            List<(int order, MethodInfo method)> listeners = new();
            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<OrderedInitializeOnLoadAttribute>())
            {
                if (!method.IsStatic)
                {
                    Debug.LogError($"[JanSharpCommon] Attempt to use the {nameof(OrderedInitializeOnLoadAttribute)} "
                        + $"on a non static method.");
                    continue;
                }
                if (method.GetParameters().Length != 0)
                {
                    Debug.LogError($"[JanSharpCommon] Attempt to use the {nameof(OrderedInitializeOnLoadAttribute)} "
                        + $"on a method with parameters. It must not have any parameters.");
                    continue;
                }
                listeners.Add((method.GetCustomAttribute<OrderedInitializeOnLoadAttribute>().Order, method));
            }
            foreach (var listener in listeners.OrderBy(l => l.order))
                listener.method.Invoke(null, null);
        }

        private static ReadOnlyCollection<System.Type> allUdonSharpBehaviourTypes = null;
        public static ReadOnlyCollection<System.Type> AllUdonSharpBehaviourTypes
        {
            get
            {
                if (allUdonSharpBehaviourTypes != null)
                    return allUdonSharpBehaviourTypes;
                // TypeCache.GetTypesDerivedFrom<UdonSharpBehaviour>(); // TODO: Does this contain abstract classes?
                // System.Diagnostics.Stopwatch sw = new();
                // sw.Start();
                allUdonSharpBehaviourTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => IsCustomAssemblyWeAreInterestedIn(a))
                    .SelectMany(d => d.GetTypes())
                    .Where(t => typeof(UdonSharpBehaviour).IsAssignableFrom(t))
                    .ToList()
                    .AsReadOnly();
                // Debug.Log($"Checking types took: {sw.Elapsed}, Found {AllUdonSharpBehaviourTypes.Count} UdonSharpBehaviour deriving classes");
                // foreach (System.Type t in AllUdonSharpBehaviourTypes)
                //     Debug.Log(t.Name);
                return allUdonSharpBehaviourTypes;
            }
        }

        private static bool IsInNamespace(string assemblyName, string namespaceName, bool isVRC = false)
        {
            return assemblyName == namespaceName
                || assemblyName.StartsWith(namespaceName + ".")
                || (isVRC && assemblyName == (namespaceName + "-Editor"));
        }

        private static bool IsCustomAssemblyWeAreInterestedIn(Assembly assembly)
        {
            string name = assembly.GetName().Name;
            return !IsInNamespace(name, "Unity")
                && !IsInNamespace(name, "UnityEngine")
                && !IsInNamespace(name, "UnityEditor")
                && !IsInNamespace(name, "System")
                && !IsInNamespace(name, "Microsoft")
                && !IsInNamespace(name, "VRC", isVRC: true)
                && !IsInNamespace(name, "VRCSDKBase", isVRC: true)
                && !IsInNamespace(name, "VRCSDK3", isVRC: true)
                && !IsInNamespace(name, "VRCCore", isVRC: true)
                && !IsInNamespace(name, "vpm-core-lib", isVRC: true)
                && !IsInNamespace(name, "UdonSharp")
                && !IsInNamespace(name, "UniTask")
                && !IsInNamespace(name, "Newtonsoft")
                && !IsInNamespace(name, "Mono")
                && !IsInNamespace(name, "YamlDotNet")
                && !IsInNamespace(name, "Cinemachine")
                && !name.StartsWith("com.unity.")
                && name != "mscorlib"
                && name != "netstandard"
                && name != "nunit.framework"
                && name != "0Harmony";
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OrderedInitializeOnLoadAttribute : System.Attribute
    {
        // See the attribute guidelines at http://go.microsoft.com/fwlink/?LinkId=85236
        public int Order { get; set; } = 0;
        public OrderedInitializeOnLoadAttribute() { }
    }
}
