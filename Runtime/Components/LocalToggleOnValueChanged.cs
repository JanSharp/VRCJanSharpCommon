using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalToggleOnValueChanged : UdonSharpBehaviour
    {
        public Toggle toggle;
        [Tooltip("When true, an active toggle means all Objects To Toggle will be disabled instead.")]
        public bool inverted;
        [Tooltip("By toggle it really means setting the active state equal to isOn state of the toggle. Inverted inverts the state, naturally.")]
        public GameObject[] objectsToToggle;

        public void OnValueChanged()
        {
            if (toggle != null && objectsToToggle != null)
                foreach (GameObject obj in objectsToToggle)
                    obj.SetActive(toggle.isOn != inverted);
        }
    }
}
