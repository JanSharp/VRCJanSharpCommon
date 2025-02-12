using UdonSharp;

namespace JanSharp
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SingletonScriptAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        private readonly string prefabGuid;
        public string PrefabGuid => prefabGuid;

        /// <summary>
        /// <para>Classes (deriving from <see cref="UdonSharpBehaviour"/>) marked with this attribute are
        /// enforced to only exist zero or one times in a scene, never more.</para>
        /// <para>Any script which has fields marked with <see cref="SingletonReferenceAttribute"/> attribute
        /// will get those fields set as a reference to the singleton instance of the script marked with
        /// <see cref="SingletonScriptAttribute"/> upon entering play mode or building the world.</para>
        /// </summary>
        public SingletonScriptAttribute(string prefabGuid)
        {
            this.prefabGuid = prefabGuid;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class SingletonReferenceAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        public bool Optional { get; set; }

        /// <summary>
        /// <para>Fields marked with this attribute will get as a reference to the singleton instance of the
        /// script marked with <see cref="SingletonScriptAttribute"/> corresponding to this field's type upon
        /// entering play mode or building the world.</para>
        /// <para>Non optional script references automatically cause the prefab of the dependency to get
        /// instantiated at build time (entering play mode or building the VRChat world).</para>
        /// </summary>
        public SingletonReferenceAttribute()
        { }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class SingletonDependencyAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        private readonly System.Type singletonType;
        public System.Type SingletonType => singletonType;

        /// <summary>
        /// <para>The given <paramref name="singletonType"/> must be the type of a script which has the
        /// <see cref="SingletonScriptAttribute"/>.</para>
        /// <para>This will cause the prefab of the dependency to get instantiated at build time (entering play
        /// mode or building the VRChat world).</para>
        /// </summary>
        /// <param name="singletonType"></param>
        public SingletonDependencyAttribute(System.Type singletonType)
        {
            this.singletonType = singletonType;
        }
    }
}
