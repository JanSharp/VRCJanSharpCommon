using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using JetBrains.Annotations;

namespace JanSharp
{
    [RequireComponent(typeof(VRC.SDK3.Components.VRCPickup))]
    public class PickupSendLocalEvent : UdonSharpBehaviour
    {
        [Header("Make sure to set sync mode to None", order = 0)]
        [Space(-10, order = 1)]
        [Header("unless this is on an item with a VRC Object Sync", order = 2)]
        [Space(-10, order = 3)]
        [Header("in which case it should be Continuous.", order = 4)]
        [Space(-10, order = 5)]
        [Header("For other synced scripts, match their sync mode.", order = 6)]
        [Space(order = 7)]
        [Tooltip("Can be null.")]
        /// <summary>Can be null.</summary>
        [PublicAPI] public UdonBehaviour onPickupUseDownTarget;
        [Tooltip("If On Pickup Use Down Target is not null, this should not be empty.")]
        /// <summary>If onPickupUseDownTarget is not null, this should not be empty.</summary>
        [PublicAPI] public string onPickupUseDownEventName;
        [Space]
        [Tooltip("Can be null.")]
        /// <summary>Can be null.</summary>
        [PublicAPI] public UdonBehaviour onPickupUseUpTarget;
        [Tooltip("If On Pickup Use Up Target is not null, this should not be empty.")]
        /// <summary>If onPickupUseUpTarget is not null, this should not be empty.</summary>
        [PublicAPI] public string onPickupUseUpEventName;
        [Space]
        [Tooltip("Can be null.")]
        /// <summary>Can be null.</summary>
        [PublicAPI] public UdonBehaviour onPickupTarget;
        [Tooltip("If On Pickup Target is not null, this should not be empty.")]
        /// <summary>If onPickupTarget is not null, this should not be empty.</summary>
        [PublicAPI] public string onPickupEventName;
        [Space]
        [Tooltip("Can be null.")]
        /// <summary>Can be null.</summary>
        [PublicAPI] public UdonBehaviour onDropTarget;
        [Tooltip("If On Drop Target is not null, this should not be empty.")]
        /// <summary>If onDropTarget is not null, this should not be empty.</summary>
        [PublicAPI] public string onDropEventName;

        public override void OnPickupUseDown()
        {
            if (onPickupUseDownTarget != null)
                onPickupUseDownTarget.SendCustomEvent(onPickupUseDownEventName);
        }

        public override void OnPickupUseUp()
        {
            if (onPickupUseUpTarget != null)
                onPickupUseUpTarget.SendCustomEvent(onPickupUseUpEventName);
        }

        public override void OnPickup()
        {
            if (onPickupTarget != null)
                onPickupTarget.SendCustomEvent(onPickupEventName);
        }

        public override void OnDrop()
        {
            if (onDropTarget != null)
                onDropTarget.SendCustomEvent(onDropEventName);
        }
    }
}
