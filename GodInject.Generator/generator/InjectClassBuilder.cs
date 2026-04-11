using GodInject.Generator.generator.data;
using GodInject.Generator.utils;
using Microsoft.CodeAnalysis;

namespace GodInject.Generator.generator
{
    /// <summary>
    /// Builds the Injection generation output
    /// </summary>
    public class InjectClassBuilder
    {
        public InjectResolverBuilder InjectResolverBuilder { get; set; } = new InjectResolverBuilder();

        /// <summary>
        /// Creates a full class output from the class symbol, injected members and other settings
        /// </summary>
        /// <param name="classSymbol">Symbol of the class</param>
        /// <param name="dataMembers">Injected data members (properties/fields)</param>
        /// <param name="addInjectMethod">Whether it should add a manual injection method</param>
        /// <param name="addConstructor">Whether it should inject in the constructor</param>
        /// <returns>The string class ready to save</returns>
        public string CreateClass(INamedTypeSymbol classSymbol, InjectedDataMembers dataMembers, bool addInjectMethod, bool addConstructor) 
        {
            string classBody = "";
            string resolvingCalls = InjectResolverBuilder.GetTextResolvingDataMembers(dataMembers);
            resolvingCalls = TextHelper.FormatNewLines(
                "try",
                "{",
                    resolvingCalls,
                "}",
                "catch(Exception ex)",
                "{",
                    "throw new Exception(\"Resolving dependencies in a generated class failed. Look at the inner exception for more info\", ex);",
                "}"
            );

            bool hasNotificationMethod = SymbolHelper.HasNotOverridenVirtualMethodAccess(classSymbol, Constants.GODOT_NOTIFICATION_METHOD);
            if (hasNotificationMethod)
            {
                addConstructor = false;
                if (addInjectMethod)
                    classBody += GetNotificationMethod(classSymbol, $"{Constants.MANAGED_FUNCTION_NAME}();") + "\n";
                else
                    classBody += GetNotificationMethod(classSymbol, resolvingCalls);
            }

            if (addConstructor)
                if (addInjectMethod)
                    classBody += GetConstructor(classSymbol.Name, $"{Constants.MANAGED_FUNCTION_NAME}();") + "\n";
                else
                    classBody += GetConstructor(classSymbol.Name, resolvingCalls);

            if (addInjectMethod)
                classBody += GetManagedInjectionMethod(resolvingCalls);
            

            return TextIndentor.IndentTextLikeCSharp(FormatClass(classSymbol, classBody));
        }

        /// <summary>
        /// Formats the class in a predictable way.
        /// </summary>
        /// <param name="classSymbol">Symbol of the class</param>
        /// <param name="classBody">Class body</param>
        /// <returns>Formatted class string</returns>
        private string FormatClass(INamedTypeSymbol classSymbol, string classBody)
        {
            string? namespacePart = classSymbol.ContainingNamespace.IsGlobalNamespace || classSymbol.ContainingNamespace == null
                ? null : $"namespace {classSymbol.ContainingNamespace};";
            string? injectClass = TextHelper.FormatNewLines(
                "#nullable enable",
                "using System;",
                $"using {Constants.INJECT_CONTAINER_NAMESPACE};",
                namespacePart,
                $"public partial class {classSymbol.Name}",
                $"{{",
                $"{classBody}",
                $"}}"
            );
            return injectClass.ToString();
        }

        /// <summary>
        /// Creates a notification method that allows injection through the _Notification in Godot
        /// </summary>
        /// <param name="classSymbol">Symbol of the class</param>
        /// <param name="resolvingText">Text resolving the data members marked with [Inject]</param>
        /// <returns>String with the notification resolution</returns>
        private string GetNotificationMethod(INamedTypeSymbol classSymbol, string resolvingText)
        {
            string? userNotificationCall = null;
            if(SymbolHelper.HasMethod(classSymbol, Constants.USER_NOTIFICATION_METHOD))
            {
                userNotificationCall = $"{Constants.USER_NOTIFICATION_METHOD}({Constants.GODOT_NOTIFICATION_ARG_NAME});";
            }

            string args = Constants.GODOT_NOTIFICATION_ARG_TYPE + " " + Constants.GODOT_NOTIFICATION_ARG_NAME;
            return TextHelper.FormatNewLines(
                $"public override void {Constants.GODOT_NOTIFICATION_METHOD}({args})",
                $"{{",
                    $"base.{Constants.GODOT_NOTIFICATION_METHOD}({Constants.GODOT_NOTIFICATION_ARG_NAME});",
                    $"if({Constants.GODOT_NOTIFICATION_ARG_NAME} == {Constants.GODOT_ENTERTREE_NOTIFICATION_VAL})",
                    $"{{",
                    $"{resolvingText}",
                    $"}}",
                    userNotificationCall,
                $"}}"
            );
        }

        /// <summary>
        /// Creates a constructor using the class name and constructor body.
        /// </summary>
        /// <param name="className">The class name</param>
        /// <param name="constructorBody">The body of the constructor</param>
        /// <returns>The constructormm</returns>
        private string GetConstructor(string className, string constructorBody)
        {
            return TextHelper.FormatNewLines(
                $"public {className}()",
                $"{{",
                $"{constructorBody}",
                $"}}"
            );
        }

        /// <summary>
        /// Creates a manual injection method and method body
        /// </summary>
        /// <param name="methodBody">The body of the method</param>
        /// <returns>The manual injection method</returns>
        private string GetManagedInjectionMethod(string methodBody)
        {
            return TextHelper.FormatNewLines(
                $"public void {Constants.MANAGED_FUNCTION_NAME}() {{",
                $"{methodBody}",
                $"}}"
            );
        }
    }
}