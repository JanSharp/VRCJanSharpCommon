using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LogToggleValueChange : UdonSharpBehaviour
    {
        public Toggle toggle;

        public void OnValueChanged()
        {
            Debug.Log($"<dlt> name: {name}, new toggle value: {toggle.isOn}");
        }
    }
}
