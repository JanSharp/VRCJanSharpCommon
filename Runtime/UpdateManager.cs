using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UpdateManager : UdonSharpBehaviour
    {
        private const string InternalIndexFieldName = "customUpdateInternalIndex";
        private const string CustomUpdateMethodName = "CustomUpdate";
        private const int InitialListenersLength = 128;

        // can't use UdonBehaviour nor UdonSharpBehaviour arrays because it's not supported
        private Component[] listeners = new Component[InitialListenersLength];
        private int listenerCount = 0;

        private void Update()
        {
            for (int i = 0; i < listenerCount; i++)
            {
                ((UdonSharpBehaviour)listeners[i]).SendCustomEvent(CustomUpdateMethodName);
            }
        }

        public void Register(UdonSharpBehaviour listener)
        {
            if ((int)listener.GetProgramVariable(InternalIndexFieldName) != 0)
                return;
            if (listenerCount == listeners.Length)
                GrowListeners();
            listeners[listenerCount] = listener;
            listener.SetProgramVariable(InternalIndexFieldName, listenerCount + 1);
            listenerCount++;
        }

        public void Deregister(UdonSharpBehaviour listener)
        {
            int index = (int)listener.GetProgramVariable(InternalIndexFieldName) - 1;
            if (index == -1)
                return;
            listener.SetProgramVariable(InternalIndexFieldName, 0);
            // move current top into the gap
            listenerCount--;
            if (index != listenerCount)
            {
                listeners[index] = listeners[listenerCount];
                ((UdonSharpBehaviour)listeners[index]).SetProgramVariable(InternalIndexFieldName, index + 1);
            }
            listeners[listenerCount] = null;
        }

        private void GrowListeners()
        {
            Component[] grownListeners = new Component[listeners.Length * 2];
            listeners.CopyTo(grownListeners, 0);
            listeners = grownListeners;
        }
    }
}
