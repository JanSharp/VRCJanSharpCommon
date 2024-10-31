using UdonSharp;

namespace JanSharp
{
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public abstract class CustomRaisedEventBaseAttribute : System.Attribute
    {
        // See the attribute guidelines at
        // http://go.microsoft.com/fwlink/?LinkId=85236

        // Can't use properties because there's a bug with UdonSharp where the base class of custom attributes,
        // which this attribute class here ends up being, get treated as an "imported named type" or so and it
        // does not support properties for those types.
        // So these fields are capitalized since we can't make properties for them. They are basically acting
        // like properties, as much as they can.
        public readonly int CustomEventTypeEnumValue;
        public int Order = 0; // Named parameter. Again, not a property even though it should be, but I can't.

        protected CustomRaisedEventBaseAttribute(int customEventTypeEnumValue)
        {
            CustomEventTypeEnumValue = customEventTypeEnumValue;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class CustomRaisedEventsDispatcherAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        private readonly System.Type customRaisedEventAttributeType;
        public System.Type CustomRaisedEventAttributeType => customRaisedEventAttributeType;
        private readonly System.Type customEventEnumType;
        public System.Type CustomEventEnumType => customEventEnumType;

        public CustomRaisedEventsDispatcherAttribute(System.Type customRaisedEventAttributeType, System.Type customEventEnumType)
        {
            this.customRaisedEventAttributeType = customRaisedEventAttributeType;
            this.customEventEnumType = customEventEnumType;
        }
    }

    public static class CustomRaisedEvents
    {
        public static void Raise(ref UdonSharpBehaviour[] listeners, string eventName)
        {
            int destroyedCount = 0;
            foreach (UdonSharpBehaviour listener in listeners)
                if (listener == null)
                    destroyedCount++;
                else
                    listener.SendCustomEvent(eventName);
            if (destroyedCount == 0)
                return;

            int length = listeners.Length;
            UdonSharpBehaviour[] newListeners = new UdonSharpBehaviour[length - destroyedCount];
            int j = 0;
            for (int i = 0; i < length; i++)
            {
                UdonSharpBehaviour listener = listeners[i];
                if (listener != null)
                    newListeners[j++] = listener;
            }
            listeners = newListeners;
        }
    }
}
