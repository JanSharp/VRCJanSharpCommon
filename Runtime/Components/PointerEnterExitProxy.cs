using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.EventSystems;
using VRC.Udon;

namespace JanSharp
{
    [RequireComponent(typeof(EventTrigger))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PointerEnterExitProxy : UdonSharpBehaviour
    {
        private bool isHovering;
        public bool IsHovering => isHovering;

        [Tooltip("Can be null.")]
        /// <summary>Can be <see langword="null"/>.</summary>
        [PublicAPI] public UdonBehaviour onPointerEnterTarget;
        [Tooltip("If On Pointer Enter Target is not null, this should not be empty.")]
        /// <summary>If <see cref="onPointerEnterTarget"/> is not <see langword="null"/>, this should not be empty.</summary>
        [PublicAPI] public string onPointerEnterEventName = "OnPointerEnter";
        [Space]
        [Tooltip("Can be null.")]
        /// <summary>Can be <see langword="null"/>.</summary>
        [PublicAPI] public UdonBehaviour onPointerExitTarget;
        [Tooltip("If On Pointer Exit Target is not null, this should not be empty.")]
        /// <summary>If <see cref="onPointerExitTarget"/> is not <see langword="null"/>, this should not be empty.</summary>
        [PublicAPI] public string onPointerExitEventName = "OnPointerExit";

        private void OnDisable()
        {
            // The exit event does not get raised if the pointer is
            // inside of the element while it gets deactivated.
            OnPointerExit();
        }

        public void OnPointerEnter()
        {
            if (isHovering)
                return;
            isHovering = true;
            if (onPointerEnterTarget != null)
                onPointerEnterTarget.SendCustomEvent(onPointerEnterEventName);
        }

        public void OnPointerExit()
        {
            if (!isHovering)
                return;
            isHovering = false;
            if (onPointerExitTarget != null)
                onPointerExitTarget.SendCustomEvent(onPointerExitEventName);
        }
    }
}
