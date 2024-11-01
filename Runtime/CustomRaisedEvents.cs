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
        /// <summary>
        /// <para>The lower the order the sooner this event handler shall be called when the event gets
        /// raised.</para>
        /// <para>If registrations share the same order then their order of execution is undefined.</para>
        /// </summary>
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

        /// <summary>
        /// <para>Marks an <see cref="UdonSharpBehaviour"/> class as a dispatcher of custom raised events.</para>
        /// <para>This class must have a serialized field (so public or using
        /// <see cref="UnityEngine.SerializeField"/>) field with the type <see cref="UdonSharpBehaviour[]"/>
        /// for each member of the <paramref name="customEventEnumType"/>.</para>
        /// <para>The names of each of those fields must match the names of the enum members, however with the
        /// first letter converted to lower case and <c>"Listeners"</c> appended to the name.</para>
        /// <para>For most cases <see cref="CustomRaisedEvents.Raise(ref UdonSharpBehaviour[], string)"/> can
        /// be used to raise the events at runtime - make sure to use <c>nameof()</c> using the enum members
        /// as the eventName argument.</para>
        /// </summary>
        /// <param name="customRaisedEventAttributeType">An attribute type which must derive from
        /// <see cref="CustomRaisedEventBaseAttribute"/>. This is the attribute which gets applied to
        /// listener methods, and it should therefore have 1 positional argument, which uses
        /// <paramref name="customEventEnumType"/>. And then as part of the constructor simply casts that
        /// value to an <see cref="int"/>, like for example: <c>: base((int)eventType)</c>.</param>
        /// <param name="customEventEnumType">The type of an enum where each enum member/field is the name of
        /// a custom event which should be able to be listened to by other scripts.</param>
        public CustomRaisedEventsDispatcherAttribute(System.Type customRaisedEventAttributeType, System.Type customEventEnumType)
        {
            this.customRaisedEventAttributeType = customRaisedEventAttributeType;
            this.customEventEnumType = customEventEnumType;
        }
    }

    public static class CustomRaisedEvents
    {
        /// <summary>
        /// <para>Calls <see cref="UdonSharpBehaviour.SendCustomEvent(string)"/> on each
        /// <paramref name="listeners"/> using the <paramref name="eventName"/> as the sent custom event
        /// name.</para>
        /// <para>Make sure to use <c>nameof()</c> using the event type enum member/field for
        /// <paramref name="eventName"/>.</para>
        /// </summary>
        /// <param name="listeners">Takes a reference because any listeners which get deleted at runtime get
        /// removed from the array (and the array gets replaced with a shorter one) just as a small
        /// optimization.</param>
        /// <param name="eventName">The name of the event to be raised, should match one of the event type
        /// enum fields/members, so make sure to use <c>nameof()</c>.</param>
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
