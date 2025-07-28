using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AlwaysActive]
    public class AlwaysActiveTest : UdonSharpBehaviour
    {
        private void Start()
        {
            Debug.Log("[JanSharpCommon] AlwaysActiveTest  Start");
        }
    }
}
