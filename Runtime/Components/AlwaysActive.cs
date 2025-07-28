using UnityEngine;
using VRC.SDKBase;

namespace JanSharp
{
    public class AlwaysActive : MonoBehaviour, IEditorOnly { }

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class AlwaysActiveAttribute : System.Attribute
    {
        /// <summary>
        /// <para>The object this script is on will be moved on Start to become a child of the
        /// <see cref="AlwaysActiveManager"/>.</para>
        /// <para>This object can be disabled on Start and it will still get moved, because it is the Start
        /// event of the <see cref="AlwaysActiveManager"/> that moves this object.</para>
        /// <para>The manager gets instantiated into the scene at build time if it is missing. This attribute
        /// is effectively like a <see cref="SingletonDependencyAttribute"/> targeting the
        /// <see cref="AlwaysActiveManager"/>.</para>
        /// <para>The transform values of the objects getting moved must not matter.</para>
        /// </summary>
        public AlwaysActiveAttribute() { }
    }
}
