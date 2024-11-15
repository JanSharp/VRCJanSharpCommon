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

        public virtual void WannaBeConstructor() { }
        public virtual void WannaBeDestructor() { }
        public void Delete() => WannaBeClasses.Delete(this);
    }
}
