using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TestWannaBe : UdonSharpBehaviour
    {
        [HideInInspector] [SerializeField] [SingletonReference] private WannaBeClassesManager wannaBeClasses;

        private void Start()
        {
            TestWannaBeClass testInstOne = wannaBeClasses.New<TestWannaBeClass>(nameof(TestWannaBeClass));
            testInstOne.playerName = "Foo";
            TestWannaBeClass testInstTwo = wannaBeClasses.New<TestWannaBeClass>(nameof(TestWannaBeClass));
            testInstTwo.playerName = "bar";
            testInstTwo.health = 50;
            testInstOne.Print();
            testInstTwo.Print();
        }
    }
}
