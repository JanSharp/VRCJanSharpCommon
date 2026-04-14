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
        /// <summary>
        /// <para>Used by the <see cref="WannaBeClassesManager"/>.</para>
        /// </summary>
        [HideInInspector][SerializeField] private int internalClassIndex;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        /// <summary>
        /// <para>Editor only, do not use.</para>
        /// </summary>
        public int InternalClassIndex => internalClassIndex;
#endif
        private int referencesCount = 1;
        private bool hasBeenDestructed = false;

        public virtual void WannaBeConstructor() { }
        public virtual void WannaBeDestructor() { }
        public void Delete() => wannaBeClasses.Delete(this);

        /// <summary>
        /// <para>Override this and return <see langword="true"/> in order to indicate that a class supports
        /// its instances being reused.</para>
        /// <para>In other words, when this is <see langword="true"/>, class instances of this type do not get
        /// destroyed when the instance gets deleted. They never turn <see langword="null"/>. Rather the
        /// manager keeps those instances around, and when a new instance is requested to be created it will
        /// reuse an existing one if there are any.</para>
        /// <para>When this is <see langword="true"/>, <see cref="ResetWannaBeClassToDefault"/> must be
        /// implemented, see its intellisense.</para>
        /// <para>When implementing support for pooling one must be extra mindful of delayed events sent to
        /// instances of this class. Such delayed events may run on an already "destroyed" instance. Or worse,
        /// a delayed event might get sent, the class instance gets "destroyed", a new instance gets created
        /// reusing said instance, the previously mentioned delayed event finally runs. This means using
        /// <see cref="WannaBeClassExtensions.CheckLiveliness(WannaBeClass)"/> would not be a reliable way to
        /// check if a delayed event is supposed to actually run.</para>
        /// </summary>
        public virtual bool WannaBeClassSupportsPooling => false;
        /// <summary>
        /// <para>When <see cref="WannaBeClassSupportsPooling"/> is <see langword="true"/>, this method must
        /// be overridden and used to set all the variables of the class to the same values as the field
        /// initializers do. Ensure to truly reset all variables, after
        /// <see cref="ResetWannaBeClassToDefault"/> got run any and all instances of this class should look
        /// perfectly identical. Identical to both those that ran <see cref="ResetWannaBeClassToDefault"/> as
        /// well as entirely freshly created class instances. (There are some valid exceptions to this rule in
        /// cases where handling of delayed events is involved.)</para>
        /// <para>For classes supporting pooling, <see cref="ResetWannaBeClassToDefault"/> gets called when an
        /// instance of this class got destroyed. Importantly it does not run for freshly created class
        /// instances, this being the origin of the importance of matching what field initializers do.</para>
        /// </summary>
        public virtual void ResetWannaBeClassToDefault() { }

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
