using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class WannaBeClass : UdonSharpBehaviour
    {
        [HideInInspector] [SerializeField] [SingletonReference] private WannaBeClassesManager wannaBeClasses;
        public WannaBeClassesManager WannaBeClasses => wannaBeClasses;
        private int referencesCount = 1;

        public virtual void WannaBeConstructor() { }
        public virtual void WannaBeDestructor() { }
        public void Delete()
        {
            WannaBeDestructor();
            Destroy(this.gameObject);
        }

        public void IncrementRefsCount() => referencesCount++;
        public void DecrementRefsCount()
        {
            if ((--referencesCount) == 0)
                Delete();
        }
    }
}
