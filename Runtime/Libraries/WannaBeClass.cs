using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class WannaBeClass : UdonSharpBehaviour
    {
        /// <summary>
        /// <para>Read only.</para>
        /// </summary>
        [HideInInspector][SingletonReference] public WannaBeClassesManager wannaBeClasses;
        private int referencesCount = 1;
        private bool hasBeenDestructed = false;

        public virtual void WannaBeConstructor() { }
        public virtual void WannaBeDestructor() { }
        public void Delete()
        {
            if (hasBeenDestructed)
                return;
            hasBeenDestructed = true;
            WannaBeDestructor();
            Destroy(this.gameObject);
        }

        /// <summary>
        /// <para>Think about this like <c>std::move()</c>.</para>
        /// <para>Except that we are in Udon land so what this actually does is the same thing as
        /// <see cref="DecrementRefsCount"/>, except it does not immediately delete the instance of this class
        /// if the reference count dropped to 0. This means that the function the instance of this class is
        /// being passed to <b>must</b> call <see cref="IncrementRefsCount"/>.</para>
        /// <para>Otherwise if the receiving function does not wish to keep a reference to the passed in
        /// "r value reference" (pretend with me will you), it can instead call
        /// <see cref="WannaBeClassExtensions.CheckLiveliness"/>.</para>
        /// <para>This is optional because after 1 frame <see cref="WannaBeClassExtensions.CheckLiveliness"/>
        /// will be run automatically, deleting the instance if reference count has dropped to 0. By calling
        /// <see cref="WannaBeClassExtensions.CheckLiveliness"/> manually all that does is speed up the
        /// process of instance deletion as well as preventing obscure bugs where an instance suddenly gets
        /// deleted when it isn't expected, because <see cref="IncrementRefsCount"/> was not called when it
        /// should have.</para>
        /// <para>There is no C++ equivalent for that, this is just stupid. I know it, you know it.</para>
        /// </summary>
        public WannaBeClass StdMove()
        {
            referencesCount--;
            SendCustomEventDelayedFrames(nameof(CheckLivelinessInternal), 1);
            return this;
        }
        /// <summary>
        /// </summary>
        /// <returns><see langword="true"/> if the instance is still alive. When <see langword="false"/> Unity
        /// has already been instructed to destroy this game object, it will turn <see langword="null"/>
        /// soon.</returns>
        public bool CheckLivelinessInternal()
        {
            if (referencesCount <= 0)
                Delete();
            return !hasBeenDestructed;
        }
        public void IncrementRefsCount() => referencesCount++;
        public void DecrementRefsCount()
        {
            if ((--referencesCount) <= 0)
                Delete();
        }
    }

    public static class WannaBeClassExtensions
    {
        /// <summary>
        /// <para>Can be called on instances which are <see langword="null"/>.</para>
        /// </summary>
        /// <returns><see langword="true"/> if the instance is still alive. When <see langword="false"/> the
        /// instance is either already <see langword="null"/> or Unity has already been instructed to destroy
        /// this game object, in which case it will turn <see langword="null"/> soon.</returns>
        public static bool CheckLiveliness(this WannaBeClass instance)
        {
            return instance != null && instance.CheckLivelinessInternal();
        }
    }
}
