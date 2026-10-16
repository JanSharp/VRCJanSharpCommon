namespace JanSharp
{
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class FindInSceneAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        /// <summary>
        /// <para>Effectively equivalent to a
        /// <see cref="UnityEngine.Object.FindObjectsByType{T}(UnityEngine.FindObjectsInactive, UnityEngine.FindObjectsSortMode)"/>
        /// (with <see cref="UnityEngine.FindObjectsInactive.Include"/>, sort order undefined but consistent)
        /// setting the value of this field, however at build time (when entering play mode or publishing)
        /// rather than at runtime.</para>
        /// <para>Editor only objects will not be included in the list.</para>
        /// <para>Must be applied to a field which is an array of <see cref="UnityEngine.Component"/>s or a
        /// class deriving thereof.</para>
        /// </summary>
        public FindInSceneAttribute()
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class FindInChildrenAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        /// <summary>
        /// <para>Effectively equivalent to a
        /// <see cref="UnityEngine.Component.GetComponentsInChildren{T}(bool)"/> (with <c>includeInactive</c>
        /// <see langword="true"/>) setting the value of this field, however at build time (when entering play
        /// mode or publishing) rather than at runtime.</para>
        /// <para>Editor only objects will not be included in the list.</para>
        /// <para>Must be applied to a field which is an array of <see cref="UnityEngine.Component"/>s or a
        /// class deriving thereof.</para>
        /// </summary>
        public FindInChildrenAttribute()
        { }
    }
}
