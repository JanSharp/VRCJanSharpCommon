using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonScript]
    public class BuildTimeIdAssignmentTest : BuildTimeIdAssignmentTestBase
    {
        [BuildTimeIdAssignment("ids", "highestId")]
        [SerializeField] private LogToggleValueChange[] entries;
    }
}
