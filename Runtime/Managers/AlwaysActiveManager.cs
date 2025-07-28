using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-10)]
    [SingletonScript("9a6fdb1d3f0c2a45982defd03aae037a")] // Runtime/Prefabs/AlwaysActiveManager.prefab
    public class AlwaysActiveManager : UdonSharpBehaviour
    {
        [Header("List gets populated on build, cannot manually edit\n"
            + "Use the AlwaysActive script component\n"
            + "Or use the [AlwaysActive] attribute on an U# class")]
        [SerializeField] private Transform[] allTransformsToMove;

        private void Start()
        {
            Transform parent = this.transform;
            foreach (var toMove in allTransformsToMove)
                if (toMove != null)
                    toMove.SetParent(parent, worldPositionStays: false);
        }
    }
}
