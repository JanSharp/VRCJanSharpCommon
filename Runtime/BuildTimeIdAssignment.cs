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

        public BuildTimeIdAssignmentAttribute(string idsArrayFieldName, string highestIdFieldName)
        {
            this.idsArrayFieldName = idsArrayFieldName;
            this.highestIdFieldName = highestIdFieldName;
        }
    }
}
