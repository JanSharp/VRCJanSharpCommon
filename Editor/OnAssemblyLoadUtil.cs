using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UdonSharp;

namespace JanSharp
{
    [InitializeOnLoad]
    [DefaultExecutionOrder(-1500)]
    public static class OnAssemblyLoadUtil
    {
        public static ReadOnlyCollection<System.Type> AllUdonSharpBehaviourTypes { private set; get; }

        static OnAssemblyLoadUtil()
        {
            // System.Diagnostics.Stopwatch sw = new();
            // sw.Start();
            AllUdonSharpBehaviourTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => IsCustomAssemblyWeAreInterestedIn(a))
                .SelectMany(d => d.GetTypes())
                .Where(t => EditorUtil.DerivesFrom(t, typeof(UdonSharpBehaviour)))
                .ToList()
                .AsReadOnly();
            // Debug.Log($"Checking types took: {sw.Elapsed}, Found {AllUdonSharpBehaviourTypes.Count} UdonSharpBehaviour deriving classes");
            // foreach (System.Type t in AllUdonSharpBehaviourTypes)
            //     Debug.Log(t.Name);
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
}
