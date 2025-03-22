using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodInject
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// <see cref="ManagedInjectionAttribute"/> is an attribute that you can apply to your class to instruct the generator not to use the parameterless constructor. 
    /// Instead, it will generate a method called InjectAll() that the user must call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ManagedInjectionAttribute(bool allowParameterless = false) : Attribute
    {
        /// <summary>
        /// Whether parameterless constructor should be allowed anyway.
        /// </summary>
        public bool AllowParameterless { get; } = allowParameterless;
    }
#else
    /// <summary>
    /// <see cref="ManagedInjectionAttribute"/> is an attribute that you can apply to your class to instruct the generator not to use the parameterless constructor. 
    /// Instead, it will generate a method called InjectAll() that the user must call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ManagedInjectionAttribute : Attribute
    {
        public ManagedInjectionAttribute() {
            AllowParameterless = false;
        }
        public ManagedInjectionAttribute(bool allowParameterless){
            AllowParameterless = allowParameterless;
        }

        /// <summary>
        /// Whether parameterless constructor should be allowed anyway.
        /// </summary>
        public bool AllowParameterless { get; set; }
    }
#endif
}
