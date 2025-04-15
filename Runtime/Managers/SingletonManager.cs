using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonScript("ca749500abc5a4d4681a60a4ecadde80")] // Runtime/Prefabs/SingletonManager.prefab
    public class SingletonManager : UdonSharpBehaviour
    {
        [SerializeField] private UdonSharpBehaviour[] singletonInsts;
        [SerializeField] private string[] singletonClassNames;

        private DataDictionary singletonLut;

        private void PopulateLut()
        {
            singletonLut = new DataDictionary();
            int length = singletonInsts.Length;
            for (int i = 0; i < length; i++)
                singletonLut.Add(singletonClassNames[i], singletonInsts[i]);
        }

        public UdonSharpBehaviour GetSingletonDynamic(string singletonClassName)
        {
            if (singletonLut == null)
                PopulateLut();
            return singletonLut.TryGetValue(singletonClassName, out DataToken instToken)
                ? (UdonSharpBehaviour)instToken.Reference
                : null;
        }
    }

    public static class SingletonManagerExtensions
    {
        public static T GetSingleton<T>(this SingletonManager singletonManager, string singletonClassName)
            where T : UdonSharpBehaviour
        {
            return (T)singletonManager.GetSingletonDynamic(singletonClassName);
        }
    }

    public static class SingletonsUtil
    {
        public static T GetSingleton<T>(string singletonClassName)
            where T : UdonSharpBehaviour
        {
            GameObject managerGo = GameObject.Find("/SingletonManager");
            if (managerGo == null)
                return null;
            return managerGo.GetComponent<SingletonManager>().GetSingleton<T>(singletonClassName);
        }
    }
}
