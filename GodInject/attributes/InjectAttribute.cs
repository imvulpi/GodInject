using System;

namespace GodInject
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InjectAttribute(string key = null) : Attribute
    {
        public string Key { get; } = key;
    }
}
