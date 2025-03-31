using GodInject.Analyzers.generators.data;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace GodInject.Analyzers.generators.builder
{
    public class InjectClassBuilder
    {
        public const string INDENTATION = "    ";
        public string AddIntoClassWrapper(INamedTypeSymbol classSymbol, string classBody)
        {
            var injectClass =
                $"#nullable enable\n" +
                $"using System;\n" +
                $"using {Constants.INJECT_CONTAINER_NAMESPACE};\n" +
                $"{(classSymbol.ContainingNamespace == null ? "" : $"namespace {classSymbol.ContainingNamespace};")}\n" +
                $"public partial class {classSymbol.Name} {{\n" +
                $"{classBody}" +
                $"}}\n";
            return injectClass.ToString();
        }

        public string CreateClass(INamedTypeSymbol classSymbol, InjectedDataMembers dataMembers, bool addInjectMethod, bool addConstructor) 
        {
            string classBody = "";
            if (addInjectMethod)
            {
                if (addConstructor) classBody += GetConstructor(classSymbol, $"{INDENTATION}{INDENTATION}{Constants.MANAGED_FUNCTION_NAME}();", $"{INDENTATION}");
                classBody += "\n";
                classBody += GetInjectMethod(dataMembers, $"{INDENTATION}");
            }
            else
            {
                classBody += GetConstructor(classSymbol, GetTextResolvingDataMembers(dataMembers, $"{INDENTATION}{INDENTATION}"), $"{INDENTATION}");
            }

            return AddIntoClassWrapper(classSymbol, classBody);
        }

        public string GetConstructor(INamedTypeSymbol classSymbol, string constructorBody, string linePrefix = "")
        {
            string constructor =
                $"{linePrefix}public {classSymbol.Name}() {{\n" +
                $"{constructorBody}" +
                $"{linePrefix}\n" +
                $"{linePrefix}}}\n";
            return constructor;
        }

        public string GetInjectMethod(InjectedDataMembers dataMembers, string linePrefix = "")
        {
            string textsResolvingMembers = GetTextResolvingDataMembers(dataMembers, $"{linePrefix}{INDENTATION}");
            string method =
                $"{linePrefix}public void {Constants.MANAGED_FUNCTION_NAME}() {{\n" +
                $"{textsResolvingMembers}" +
                $"{linePrefix}}}\n";
            return method;
        }

        private string GetTextResolvingDataMembers(InjectedDataMembers dataMembers, string linePrefix = "")
        {
            var text = GetTextResolvingFields(dataMembers.InjectedFields, linePrefix);
            text += GetTextResolvingProperties(dataMembers.InjectedProperties, linePrefix);
            return text;
        }

        private string GetTextResolvingProperties(InjectedProperty[] injectProperties, string linePrefix = "")
        {
            var textsResolvingProperties = injectProperties.Select(prop => $"{prop.PropertySymbol.Name} = {Constants.CONTAINER_CLASS_NAME}.Resolve<{prop.PropertySymbol.Type}>({(prop.ServiceKey == null ? "" : $"\"{prop.ServiceKey}\"")});");
            string combinedTextsResolvingProperties = "";
            bool firstProperty = true;
            foreach (var text in textsResolvingProperties)
            {
                if (firstProperty)
                {
                    combinedTextsResolvingProperties += $"{linePrefix}{text}";
                    firstProperty = false;
                }
                else
                {
                    combinedTextsResolvingProperties += $"\n{linePrefix}{text}";
                }
            }
            return combinedTextsResolvingProperties;
        }

        private string GetTextResolvingFields(InjectedField[] injectFields, string linePrefix = "")
        {
            var textsResolvingProperties = injectFields.Select(field => $"{field.PropertySymbol.Name} = {Constants.CONTAINER_CLASS_NAME}.Resolve<{field.PropertySymbol.Type}>({(field.ServiceKey == null ? "" : $"\"{field.ServiceKey}\"")});");
            string combinedTextsResolvingProperties = "";
            bool firstProperty = true;
            foreach (var text in textsResolvingProperties)
            {
                if (firstProperty)
                {
                    combinedTextsResolvingProperties += $"{linePrefix}{text}";
                    firstProperty = false;
                }
                else
                {
                    combinedTextsResolvingProperties += $"\n{linePrefix}{text}";
                }
            }
            return combinedTextsResolvingProperties;
        }
    }
}