using System;

namespace GodInject
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InjectAttribute : Attribute
    {
        public InjectAttribute() { }
        public InjectAttribute(string key)
        {
            Key = key;
        }
        public string Key { get; set; }
    }
}
