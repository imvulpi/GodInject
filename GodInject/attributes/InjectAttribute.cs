namespace GodInject
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InjectAttribute : Attribute
    {
        public InjectAttribute() { }
        public InjectAttribute(object key)
        {
            Key = key;
        }
        public object Key { get; set; }
    }
}
