using GodInject.Prebuild.injection_generator.data;
using GodInject.Prebuild.utils;
using Microsoft.CodeAnalysis;

namespace GodInject.Prebuild.injection_generator
{
    public class InjectClassBuilder
    {
        public InjectResolverBuilder InjectResolverBuilder { get; set; } = new InjectResolverBuilder();

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

        private string FormatClass(INamedTypeSymbol classSymbol, string classBody)
        {
            string namespacePart = classSymbol.ContainingNamespace.IsGlobalNamespace || classSymbol.ContainingNamespace == null
                ? null : $"namespace {classSymbol.ContainingNamespace};";
            var injectClass = TextHelper.FormatNewLines(
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

        private string GetNotificationMethod(INamedTypeSymbol classSymbol, string resolvingText)
        {
            string userNotificationCall = null;
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

        private string GetConstructor(string className, string constructorBody)
        {
            return TextHelper.FormatNewLines(
                $"public {className}()",
                $"{{",
                $"{constructorBody}",
                $"}}"
            );
        }

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