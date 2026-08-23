using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UnityEditor;

namespace JanSharp.Internal
{
    public static class TrulyPostLateUpdateEditor
    {
        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
        {
            SingletonScriptEditor.RegisterCustomDependencyResolver(new Resolver());
        }

        private class Resolver : ISingletonDependencyResolver
        {
            private const BindingFlags PrivateAndPublicFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            public IEnumerable<(string fieldName, System.Type singletonType, bool optional)> Resolve(System.Type ubType)
            {
                if (EditorUtil.GetMethodsIncludingBase(ubType, PrivateAndPublicFlags, typeof(UdonSharpBehaviour))
                    .Any(m => m.GetCustomAttributes<CustomRaisedEventBaseAttribute>()
                        .Any(a => a.GetType() == typeof(OnTrulyPostLateUpdateAttribute))))
                {
                    yield return (fieldName: null, singletonType: typeof(TrulyPostLateUpdateManager), optional: false);
                }
            }
        }
    }
}
