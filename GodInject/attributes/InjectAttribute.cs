using System;

namespace GodInject
{
#if NET8_0_OR_GREATER
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InjectAttribute(string key = null) : Attribute
    {
        public string Key { get; } = key;
    }
#else
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InjectAttribute : Attribute
    {
        public InjectAttribute(){}
        public InjectAttribute(string key){
            Key = key;
        }
        public string Key { get; set; }
    }
#endif
}
