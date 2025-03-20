using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonScript("0d045dda757c0a5f7872bc541f866f61")] // Runtime/Prefabs/UpdateManager.prefab
    public class UpdateManager : UdonSharpBehaviour
    {
        private const string InternalIndexFieldName = "customUpdateInternalIndex";
        private const string CustomUpdateMethodName = "CustomUpdate";
        private const int InitialListenersCapacity = 8;

        private UdonSharpBehaviour[] listeners = new UdonSharpBehaviour[InitialListenersCapacity];
        private int listenerCount = 0;

        private void Update()
        {
            for (int i = 0; i < listenerCount; i++)
                listeners[i].SendCustomEvent(CustomUpdateMethodName);
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
                listeners[index].SetProgramVariable(InternalIndexFieldName, index + 1);
            }
            listeners[listenerCount] = null;
        }

        private void GrowListeners()
        {
            UdonSharpBehaviour[] grownListeners = new UdonSharpBehaviour[listeners.Length * 2];
            listeners.CopyTo(grownListeners, 0);
            listeners = grownListeners;
        }
    }
}
