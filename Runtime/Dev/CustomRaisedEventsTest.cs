using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    public enum MyCustomEventType
    {
        OnFoo,
        OnBar,
    }

    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class MyCustomEvent : CustomRaisedEventBaseAttribute
    {
        public MyCustomEvent(MyCustomEventType eventType)
            : base((int)eventType)
        { }
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [CustomRaisedEventsDispatcher(typeof(MyCustomEvent), typeof(MyCustomEventType))]
    [SingletonScript("3f80fe50ad90421aba05de2d4cc91b12")] // Runtime/Dev/CustomRaisedEventsTest.prefab
    public class CustomRaisedEventsTest : UdonSharpBehaviour
    {
        [HideInInspector] [SerializeField] private UdonSharpBehaviour[] onFooListeners;
        [HideInInspector] [SerializeField] private UdonSharpBehaviour[] onBarListeners;
        [SingletonReference] public CustomRaisedEventsTest singletonRef;

        private void Start()
        {
            Debug.Log("Raising OnFoo...");
            CustomRaisedEvents.Raise(ref onFooListeners, nameof(MyCustomEventType.OnFoo));
            Debug.Log("Raising OnBar...");
            CustomRaisedEvents.Raise(ref onBarListeners, nameof(MyCustomEventType.OnBar));
        }

        [MyCustomEvent(MyCustomEventType.OnFoo, Order = 1)]
        public void OnFoo()
        {
            Debug.Log("Hello World.");
        }
    }
}
