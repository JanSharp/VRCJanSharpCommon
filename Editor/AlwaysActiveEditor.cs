using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace JanSharp.Internal
{
    [InitializeOnLoad]
    public static class AlwaysActiveOnBuild
    {
        private static List<Transform> allAlwaysActives = new();

        static AlwaysActiveOnBuild()
        {
            SingletonScriptEditor.RegisterCustomDependencyResolver(new Resolver());
            OnBuildUtil.RegisterAction(OnPreBuild, order: -143);
            foreach (System.Type type in OnAssemblyLoadUtil.AllUdonSharpBehaviourTypes.Where(HasAlwaysActiveAttribute))
                OnBuildUtil.RegisterTypeCumulative(type, OnAlwaysActiveAttributeBuild, order: -142);
            OnBuildUtil.RegisterTypeCumulative<AlwaysActive>(OnAlwaysActiveBuild, order: -141);
            OnBuildUtil.RegisterType<AlwaysActiveManager>(OnManagerBuild, order: -140);
        }

        private static bool HasAlwaysActiveAttribute(System.Type type)
            => type.GetCustomAttribute<AlwaysActiveAttribute>(inherit: true) != null;

        private class Resolver : ISingletonDependencyResolver
        {
            public IEnumerable<(string fieldName, System.Type singletonType, bool optional)> Resolve(System.Type ubType)
            {
                if (HasAlwaysActiveAttribute(ubType))
                    yield return (fieldName: null, singletonType: typeof(AlwaysActiveManager), optional: false);
            }
        }

        private static bool OnPreBuild()
        {
            allAlwaysActives.Clear();
            return true;
        }

        private static bool OnAlwaysActiveAttributeBuild(ReadOnlyCollection<Component> thing)
        {
            allAlwaysActives.AddRange(thing.Select(c => c.transform));
            return true;
        }

        private static bool OnAlwaysActiveBuild(IEnumerable<AlwaysActive> alwaysActives)
        {
            allAlwaysActives.AddRange(alwaysActives.Select(a => a.transform));
            if (alwaysActives.Any()) // The dependency resolver handles all other scripts that are using the attribute.
                SingletonScriptEditor.EnsureSingletonExists<AlwaysActiveManager>();
            return true;
        }

        private static bool OnManagerBuild(AlwaysActiveManager manager)
        {
            SerializedObject so = new SerializedObject(manager);
            EditorUtil.SetArrayProperty(
                so.FindProperty("allTransformsToMove"),
                // Due to inheritance a script could end up in multiple OnAlwaysActiveAttributeBuild
                // Similarly a class could have both the AlwaysActive attribute and the RequireComponent attribute
                allAlwaysActives.Distinct().ToList(),
                (p, v) => p.objectReferenceValue = v);
            so.ApplyModifiedProperties();
            allAlwaysActives.Clear(); // Cleanup no longer needed references.
            return true;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(AlwaysActive))]
    public class AlwaysActiveEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Label("The object this script is on will be moved on Start to become a child of the AlwaysActiveManager.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("This object can be disabled on Start and it will still get moved, because it is the Start event of the AlwaysActiveManager that moves this object.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("The manager gets instantiated into the scene at build time if it is missing.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("The transform values of the objects getting moved must not matter.", EditorStyles.wordWrappedLabel);
        }
    }
}
