using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TestWannaBeClass : WannaBeClass
    {
        [System.NonSerialized] public string playerName;
        [System.NonSerialized] public int health;

        public override void WannaBeConstructor()
        {
            playerName = "Unknown";
            health = 100;
            Debug.Log("Constructor called.");
        }

        public override void WannaBeDestructor()
        {
            Debug.Log($"Destructor for player {playerName} called.");
        }

        public void Print()
        {
            Debug.Log($"Player Name: {playerName}, Health: {health}");
        }
    }
}
