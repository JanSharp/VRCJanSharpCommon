using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TestDataStream : UdonSharpBehaviour
    {
        private void Start()
        {
            byte[] stream = new byte[64];
            int size = 0;
            int position = 0;
            {
                decimal expected = 1000m;
                DataStream.Write(ref stream, ref size, expected);
                decimal got = DataStream.ReadDecimal(stream, ref position);
                Debug.Log($"TestDataStream decimal: expected: {expected}, got: {got}, success: {got == expected}");
            }
        }
    }
}
