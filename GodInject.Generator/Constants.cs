namespace GodInject.Generator
{
    /// <summary>
    /// Constants that are used in generation output
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Namespace of the Injection Container
        /// </summary>
        public const string INJECT_CONTAINER_NAMESPACE = "GodInject.container";
        /// <summary>
        /// injection attribute full name
        /// </summary>
        public const string INJECT_ATTRIBUTE_NAMESPACE = "GodInject.InjectAttribute";
        /// <summary>
        /// Managed injection attribute full name
        /// </summary>
        public const string MANAGED_INJECT_ATTRIBUTE_NAMESPACE = "GodInject.ManagedInjectionAttribute";
        /// <summary>
        /// Managed injection function mane
        /// </summary>
        public const string MANAGED_FUNCTION_NAME = "InjectAll";
        /// <summary>
        /// Managed injection parameterless property for allowing parameterless constructor.
        /// </summary>
        public const string MANAGED_INJECT_PARAMETERLESS_PROPERTY = "AllowParameterless";
        /// <summary>
        /// Injection Container class name
        /// </summary>
        public const string CONTAINER_CLASS_NAME = "InjectContainer";
        /// <summary>
        /// Friendly name of the library
        /// </summary>
        public const string FRIENDLY_NAME_LIBRARY = "GodInject";

        // Godot Specific:
        /// <summary>
        /// The name of the Notification method in Godot
        /// </summary>
        public const string GODOT_NOTIFICATION_METHOD = "_Notification";
        /// <summary>
        /// The method users can use to handle notifications (because _Notification is taken)
        /// </summary>
        public const string USER_NOTIFICATION_METHOD = "HandleNotification";
        /// <summary>
        /// The first argument type of _Notification method
        /// </summary>
        public const string GODOT_NOTIFICATION_ARG_TYPE = "int";
        /// <summary>
        /// the first arguments name of _Notification method
        /// </summary>
        public const string GODOT_NOTIFICATION_ARG_NAME = "what";
        /// <summary>
        /// Value of the _ENTER_TREE Noticitaion from <c>what</c> first argument in Godot's Notification method
        /// </summary>
        public const string GODOT_ENTERTREE_NOTIFICATION_VAL = "10";
    }
}
