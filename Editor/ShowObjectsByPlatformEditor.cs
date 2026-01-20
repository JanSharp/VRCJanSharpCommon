using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class ShowObjectsByPlatformOnBuild
    {
        private static List<ShowObjectByPlatform> showObjectScripts;
        private static List<ShowObjectsByPlatform> showObjectsScripts;

        static ShowObjectsByPlatformOnBuild()
        {
            OnBuildUtil.RegisterTypeCumulative<ShowObjectByPlatform>(s => OnShowObjectScriptsBuild(s), order: -1204);
            OnBuildUtil.RegisterTypeCumulative<ShowObjectsByPlatform>(s => OnShowObjectsScriptsBuild(s), order: -1204);
            OnBuildUtil.RegisterType<ShowObjectsByPlatformManager>(m => OnManagerBuild(m), order: -1104);
        }

        private static bool OnShowObjectScriptsBuild(IEnumerable<ShowObjectByPlatform> scripts)
        {
            showObjectScripts = scripts.ToList();
            return true;
        }

        private static bool OnShowObjectsScriptsBuild(IEnumerable<ShowObjectsByPlatform> scripts)
        {
            showObjectsScripts = scripts.ToList();
            return true;
        }

        private static bool OnManagerBuild(ShowObjectsByPlatformManager manager)
        {
            SerializedObject so = new SerializedObject(manager);
            EditorUtil.SetArrayProperty(
                so.FindProperty("showObjectScripts"),
                showObjectScripts,
                (p, v) => p.objectReferenceValue = v);
            EditorUtil.SetArrayProperty(
                so.FindProperty("showObjectsScripts"),
                showObjectsScripts,
                (p, v) => p.objectReferenceValue = v);
            so.ApplyModifiedProperties();
            return true;
        }
    }
}
