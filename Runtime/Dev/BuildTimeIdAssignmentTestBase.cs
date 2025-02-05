using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class BuildTimeIdAssignmentTestBase : UdonSharpBehaviour
    {
        [SerializeField] private uint[] ids;
        [SerializeField] private uint highestId;
    }
}
