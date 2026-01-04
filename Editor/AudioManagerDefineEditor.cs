#if !SYLAN_AUDIOMANAGER_VERSION
using System.Collections.Generic;
using UnityEditor;

namespace JanSharp.Internal
{
    [InitializeOnLoad]
    public class AudioManagerDefineManager
    {
        static AudioManagerDefineManager()
        {
            // This only runs if SYLAN_AUDIOMANAGER_VERSION is unset, in other words only if the audio manager
            // package is not in the project. Which is perfect, exactly what we want.
            // By doing this in the on assembly load this also ensures that the define gets removed in cases
            // where the SYLAN_AUDIOMANAGER define got into the settings through any unknown means, like a
            // user doing it manually, or both the audio manager and the GM Menu having been in the project
            // before, then been removed and only the GM Menu being added back later. Basically catching edge
            // cases.
            RemoveDefinesIfPresent(EditorUserBuildSettings.selectedBuildTargetGroup, "SYLAN_AUDIOMANAGER");
        }

        private static void RemoveDefinesIfPresent(BuildTargetGroup buildTarget, params string[] definesToRemove)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            if (definesString.Length == 0)
                return;
            List<string> defines = new(definesString.Split(';'));

            bool definesChanged = false;
            foreach (string define in definesToRemove)
                if (defines.Remove(define))
                    definesChanged = true;

            if (definesChanged)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, string.Join(";", defines));
        }
    }
}
#endif
