namespace GodInject.Generator
{
    public static class Constants
    {
        public const string INJECT_CONTAINER_NAMESPACE = "GodInject.container";
        public const string INJECT_ATTRIBUTE_NAMESPACE = "GodInject.InjectAttribute";
        public const string MANAGED_INJECT_ATTRIBUTE_NAMESPACE = "GodInject.ManagedInjectionAttribute";
        public const string MANAGED_FUNCTION_NAME = "InjectAll";
        public const string MANAGED_INJECT_PARAMETERLESS_PROPERTY = "AllowParameterless";
        public const string CONTAINER_CLASS_NAME = "InjectContainer";
        public const string FRIENDLY_NAME_LIBRARY = "GodInject";
        public const string OPTION_DEBUG = "AIG_EnableDebug";
        public const string OPTION_OUTPUT = "GeneratedOutputPath";

        // Godot Specific:
        public const string GODOT_NOTIFICATION_METHOD = "_Notification";
        public const string USER_NOTIFICATION_METHOD = "HandleNotification";
        public const string GODOT_NOTIFICATION_ARG_TYPE = "int";
        public const string GODOT_NOTIFICATION_ARG_NAME = "what";
        public const string GODOT_ENTERTREE_NOTIFICATION_VAL = "10";
    }
}
