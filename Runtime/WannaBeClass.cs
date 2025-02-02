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

        /// <summary>
        /// <para>Think about this like <c>std::move()</c>.</para>
        /// <para>Except that we are in Udon land so what this actually does is the same thing as
        /// <see cref="DecrementRefsCount"/>, except it does not immediately delete the instance of this class
        /// if the reference count dropped to 0. This means that the function the instance of this class is
        /// being passed to <b>must</b> call <see cref="IncrementRefsCount"/>.</para>
        /// <para>Otherwise if the receiving function does not actually keep a reference to the passed in
        /// "r value reference" (pretend with me will you), it can instead call <see cref="CheckLiveliness"/>.
        /// This is optional because after 1 frame <see cref="CheckLiveliness"/> will be run automatically,
        /// deleting the instance if reference count has dropped to 0. Therefore by calling
        /// <see cref="CheckLiveliness"/> manually it is merely speeding up the process of instance deletion
        /// as well as preventing obscure bugs where an instance suddenly gets deleted when it isn't
        /// expected, because <see cref="IncrementRefsCount"/> was not called when it should have.</para>
        /// <para>There is no C++ equivalent for that, this is just stupid. I know it, you know it.</para>
        /// </summary>
        public WannaBeClass StdMove()
        {
            referencesCount--;
            SendCustomEventDelayedFrames(nameof(CheckLiveliness), 1);
            return this;
        }
        public void CheckLiveliness()
        {
            if (referencesCount <= 0)
                Delete();
        }
        public void IncrementRefsCount() => referencesCount++;
        public void DecrementRefsCount()
        {
            if ((--referencesCount) <= 0)
                Delete();
        }
    }
}
