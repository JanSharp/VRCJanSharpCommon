namespace JanSharp
{
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class FindInParentAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        /// <summary>
        /// <para>Effectively equivalent to a
        /// <see cref="UnityEngine.Component.GetComponentInParent{T}(bool)"/> (with <c>includeInactive</c>
        /// <see langword="true"/>) setting the value of this field, however at build time (when entering play
        /// mode or publishing) rather than at runtime.</para>
        /// </summary>
        public FindInParentAttribute()
        { }
    }
}
