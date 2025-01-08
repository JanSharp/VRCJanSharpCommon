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
        /// <para>EXcept that we are in Udon land so what this actually does is the same thing as
        /// <see cref="DecrementRefsCount"/>, except it does not delete the instance of this class if the
        /// reference count dropped to 0. This means that one <b>must</b> call
        /// <see cref="IncrementRefsCount"/> in the function the instance of this class is being passed
        /// to.</para>
        /// <para>Or if the receiving function does not actually keep a reference to the passed in
        /// "r value reference" (pretend with me will you), it <b>must instead</b> call
        /// <see cref="CheckLiveliness"/>. There is no C++ equivalent for that, this is just stupid. I know
        /// it, you know it.</para>
        /// <para>If it wasn't already easy to create memory leaks using <see cref="WannaBeClass"/>es then it
        /// certainly is when using this function.</para>
        /// </summary>
        public void StdMove() => referencesCount--;
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
