using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonScript("1250e15ce5bba49a480c6cb9115e3913")] // Runtime/Dev/BuildTimeIdAssignmentTest.prefab
    public class BuildTimeIdAssignmentTest : BuildTimeIdAssignmentTestBase
    {
        [BuildTimeIdAssignment("ids", "highestId")]
        [SerializeField] private LogToggleValueChange[] entries;
    }
}
