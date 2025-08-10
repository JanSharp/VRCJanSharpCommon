using UdonSharp;

namespace JanSharp
{
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class BuildTimeIdAssignmentAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        readonly string idsArrayFieldName;
        public string IdsArrayFieldName => idsArrayFieldName;
        readonly string highestIdFieldName;
        public string HighestIdFieldName => highestIdFieldName;

        /// <summary>
        /// <para>This attribute must be applied to a field which is serialized by unity (so public or using
        /// <see cref="UnityEngine.SerializeField"/>) which is an array of a custom
        /// <see cref="UdonSharpBehaviour"/> class.</para>
        /// <para>This field gets populated at build time with all of the instances of the given class name
        /// which exist in the scene.</para>
        /// <para>The <paramref name="idsArrayFieldName"/> is an associated array along side this array, which
        /// will contain the unique ids which have been assigned to these instances.</para>
        /// <para>An id never gets reused. Once a specific instance of a custom script has been assigned an id
        /// that id is not used for any other instance again even if the assigned instance gets deleted. This
        /// is ensured through the <paramref name="highestIdFieldName"/>.<br/>
        /// This implies that the <paramref name="highestIdFieldName"/> is not actually the highest id inside
        /// of <paramref name="idsArrayFieldName"/>, however it is the highest id which has ever been assigned
        /// to a custom script.</para>
        /// <para>Since the lowest value <paramref name="highestIdFieldName"/> could store is <c>0u</c> and
        /// the highest id never gets reused, <c>0u</c> never gets assigned to any object. It can be used as
        /// an invalid id.</para>
        /// <para>The arrays are guaranteed to be sorted by ascending ids.</para>
        /// <para>A potential use case for this attribute would be in combination with the lockstep networking
        /// package when a system wishes to support synced instances of object to exist in the scene at build
        /// time as well as be instantiated at runtime.</para>
        /// </summary>
        /// <param name="idsArrayFieldName">The <c>nameof()</c> a <see cref="uint[]"/> field.</param>
        /// <param name="highestIdFieldName">The <c>nameof()</c> a <see cref="uint"/> field.</param>
        public BuildTimeIdAssignmentAttribute(string idsArrayFieldName, string highestIdFieldName)
        {
            this.idsArrayFieldName = idsArrayFieldName;
            this.highestIdFieldName = highestIdFieldName;
        }
    }
}
