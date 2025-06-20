using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonScript("057164f89fbc47398a8142934f3f13c8")] // Runtime/Prefabs/QuickDebugUI.prefab
    public class QuickDebugUI : UdonSharpBehaviour
    {
        public GameObject desktopUIRoot;
        public RectTransform listRoot;
        public GameObject rowPrefab;

        private bool isUpdateLoopRunning = false;
        private bool UpdateLoopShouldBeRunning => allRegisteredCount != 0;

        private object[][] allRegistered = new object[ArrList.MinCapacity][];
        private int allRegisteredCount = 0;

        private GameObject[] rowRoots = new GameObject[ArrList.MinCapacity];
        private int rowRootsCount = 0;
        private Text[] rows = new Text[ArrList.MinCapacity];
        private int rowsCount = 0;
        private int activeRowsCount = 0;

        private string displayValue = "";
        public string DisplayValue
        {
            get => displayValue;
            set => displayValue = value ?? "";
        }

        public void Add(UdonSharpBehaviour script, string key, string updateFuncName)
        {
            ArrList.Add(ref allRegistered, ref allRegisteredCount, new object[] { script, key, updateFuncName });
            if (allRegisteredCount > rowsCount)
            {
                GameObject rowGo = Instantiate(rowPrefab);
                rowGo.SetActive(false);
                Transform rowTransform = rowGo.transform;
                rowTransform.SetParent(listRoot, worldPositionStays: false);
                ArrList.Add(ref rowRoots, ref rowRootsCount, rowGo);
                ArrList.Add(ref rows, ref rowsCount, rowTransform.GetComponentInChildren<Text>());
            }
            StartUpdateLoop();
        }

        public void Remove(UdonSharpBehaviour script)
        {
            int j = 0;
            for (int i = 0; i < allRegisteredCount; i++)
            {
                object[] registered = allRegistered[i];
                if (!registered[0].Equals(script))
                    allRegistered[j++] = registered;
            }
            allRegisteredCount = j;
        }

        public void Remove(UdonSharpBehaviour script, string key)
        {
            int j = 0;
            for (int i = 0; i < allRegisteredCount; i++)
            {
                object[] registered = allRegistered[i];
                if (!registered[0].Equals(script) || !registered[1].Equals(key))
                    allRegistered[j++] = registered;
            }
            allRegisteredCount = j;
        }

        private void StartUpdateLoop()
        {
            if (isUpdateLoopRunning)
                return;
            isUpdateLoopRunning = true;
            desktopUIRoot.SetActive(true);
            SendCustomEventDelayedFrames(nameof(UpdateLoop), 1);
        }

        public void UpdateLoop()
        {
            int j = 0;
            for (int i = 0; i < allRegisteredCount; i++)
            {
                object[] registered = allRegistered[i];
                UdonSharpBehaviour script = (UdonSharpBehaviour)registered[0];
                if (script == null)
                    continue;
                string key = (string)registered[1];
                string updateFuncName = (string)registered[2];
                script.SendCustomEvent(updateFuncName);
                rows[j].text = $"{script.name}: {key}: {displayValue}";
                displayValue = ""; // Reset.
                if (j >= activeRowsCount)
                    rowRoots[i].SetActive(true);
                allRegistered[j++] = registered;
            }
            allRegisteredCount = j;

            for (int i = allRegisteredCount; i < activeRowsCount; i++)
                rowRoots[i].SetActive(false);
            activeRowsCount = allRegisteredCount;

            if (!UpdateLoopShouldBeRunning) // Do this at the end so the UI gets "cleared" beforehand
            { // to prevent showing stale values for a frame when being enabled again.
                isUpdateLoopRunning = false;
                desktopUIRoot.SetActive(false);
                return;
            }

            SendCustomEventDelayedFrames(nameof(UpdateLoop), 1);
        }
    }
}
