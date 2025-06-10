using GodInject.Prebuild.generators.data;
using Microsoft.CodeAnalysis;

namespace GodInject.Prebuild.generators.builder
{
    public class InjectClassBuilder
    {
        public InjectResolverBuilder InjectResolverBuilder { get; set; } = new InjectResolverBuilder();
        public TextIndentationBuilder TextIndentationBuilder { get; set; } = new TextIndentationBuilder();
        public string AddIntoClassWrapper(INamedTypeSymbol classSymbol, string classBody)
        {
            string namespacePart = classSymbol.ContainingNamespace.IsGlobalNamespace || classSymbol.ContainingNamespace == null 
                ? null : $"namespace {classSymbol.ContainingNamespace};";
            var injectClass = BuilderHelper.FormatNewLines(
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

        public string CreateClass(INamedTypeSymbol classSymbol, InjectedDataMembers dataMembers, bool addInjectMethod, bool addConstructor) 
        {
            string classBody = "";
            string resolvingCalls = InjectResolverBuilder.GetTextResolvingDataMembers(dataMembers);
            resolvingCalls = BuilderHelper.FormatNewLines(
                "try",
                "{",
                    resolvingCalls,
                "}",
                "catch(Exception ex)",
                "{",
                    "throw new Exception(\"Resolving dependencies in a generated class failed. Look at the inner exception for more info\", ex);",
                "}"
            );

            bool hasNotificationMethod = BuilderHelper.HasNotOverridenVirtualMethod(classSymbol, Constants.GODOT_NOTIFICATION_METHOD);
            if (hasNotificationMethod)
            {
                addConstructor = false;
                classBody += GetNotificationMethod(classSymbol, resolvingCalls) + "\n";
            }

            if (addConstructor)
                if (addInjectMethod)
                    classBody += GetConstructor(classSymbol.Name, $"{Constants.MANAGED_FUNCTION_NAME}();") + "\n";
                else
                    classBody += GetConstructor(classSymbol.Name, resolvingCalls);

            if (addInjectMethod)
                classBody += GetInjectAllMethod(resolvingCalls);
            

            return TextIndentationBuilder.IndentTextLikeCSharp(AddIntoClassWrapper(classSymbol, classBody));
        }

        public string GetNotificationMethod(INamedTypeSymbol classSymbol, string resolvingText)
        {
            string userNotificationCall = null;
            if(BuilderHelper.HasMethod(classSymbol, Constants.USER_NOTIFICATION_METHOD))
            {
                userNotificationCall = $"{Constants.USER_NOTIFICATION_METHOD}({Constants.GODOT_NOTIFICATION_ARG_NAME});";
            }

            string args = Constants.GODOT_NOTIFICATION_ARG_TYPE + " " + Constants.GODOT_NOTIFICATION_ARG_NAME;
            return BuilderHelper.FormatNewLines(
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

        public string GetConstructor(string className, string constructorBody)
        {
            return BuilderHelper.FormatNewLines(
                $"public {className}()",
                $"{{",
                $"{constructorBody}",
                $"}}"
            );
        }

        public string GetInjectAllMethod(string methodBody)
        {
            return BuilderHelper.FormatNewLines(
                $"public void {Constants.MANAGED_FUNCTION_NAME}() {{",
                $"{methodBody}",
                $"}}"
            );
        }
    }
}