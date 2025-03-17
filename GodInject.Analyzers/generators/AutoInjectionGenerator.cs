using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace GodInject.Analyzers.generators
{

    public struct InjectedProperty
    {
        public IPropertySymbol PropertySymbol { get; set; }
        public string ServiceKey { get; set; }
        public InjectedProperty(IPropertySymbol symbol, string serviceKey)
        {
            PropertySymbol = symbol;
            ServiceKey = serviceKey;
        }
    }

    [Generator]
    public class AutoInjectionGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: CaptureClassSyntax,
                    transform: ConvertToClass);

            var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
            {
                var (compilation, classList) = source;
                foreach (var classSyntax in classList)
                {
                    var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
                    if (semanticModel.GetDeclaredSymbol(classSyntax) is INamedTypeSymbol classSymbol)
                    {
                        var injectAttributeSymbol = compilation.GetTypeByMetadataName(Constants.INJECT_ATTRIBUTE_NAMESPACE);
                        var className = classSymbol.Name;
                        var properties = classSymbol.GetMembers()
                                                    .OfType<IPropertySymbol>()
                                                    .Where(p => p.GetAttributes().Any(attr => SymbolEqualityComparer.IncludeNullability.Equals(attr.AttributeClass, injectAttributeSymbol)))
                                                    .ToArray();

                        var injectedProperties = new List<InjectedProperty>();
                        foreach (var property in properties)
                        {
                            var injectAttribute = property.GetAttributes().FirstOrDefault(attr => SymbolEqualityComparer.IncludeNullability.Equals(attr.AttributeClass, injectAttributeSymbol));
                            if(injectAttribute != null)
                            {
                                var serviceKey = injectAttribute.ConstructorArguments[0].IsNull ? string.Empty : (string)injectAttribute.ConstructorArguments[0].Value;
                                InjectedProperty injectedProperty = new InjectedProperty(property, serviceKey);
                                injectedProperties.Add(injectedProperty);
                            }
                        }

                        var injectedPropertiesArray = injectedProperties.ToArray();

                        if (properties.Length > 0)
                        {
                            var managedInjectAttributeSymbol = compilation.GetTypeByMetadataName(Constants.MANAGED_INJECT_ATTRIBUTE_NAMESPACE);
                            bool isManagedInject = classSymbol.GetAttributes().Any(attr => SymbolEqualityComparer.IncludeNullability.Equals(attr.AttributeClass, managedInjectAttributeSymbol));
                            var attribute = classSymbol.GetAttributes().FirstOrDefault(attr => SymbolEqualityComparer.IncludeNullability.Equals(attr.AttributeClass, managedInjectAttributeSymbol));
                            if (isManagedInject)
                            {
                                if (attribute != null && (bool)attribute.ConstructorArguments[0].Value == true) // Allow parameterless
                                {
                                    spc.AddSource($"{classSymbol.Name}.g.cs", CreateBothClass(classSymbol, injectedPropertiesArray));
                                }
                                else
                                {
                                    spc.AddSource($"{classSymbol.Name}.g.cs", CreateManagedInjectClass(classSymbol, injectedPropertiesArray));
                                }
                            }
                            else
                            {
                                // On default auto injection through a parameterless constructor
                                spc.AddSource($"{classSymbol.Name}.g.cs", CreateAutoInjectorClass(classSymbol, injectedPropertiesArray));
                            }
                        }
                    }
                }
            });
        }

        public static string CreateBothClass(INamedTypeSymbol classSymbol, InjectedProperty[] injectProperties)
        {
            var textsResolvingProperties = GetTextResolvingProperties(injectProperties, "   ");
            var partialClass =
                $"#nullable enable\n" +
                $"using System;\n" +
                $"using {Constants.INJECT_CONTAINER_NAMESPACE};\n" +
                $"namespace {classSymbol.ContainingNamespace};\n" +
                $"public partial class {classSymbol.Name} {{\n" +
                $"  public {classSymbol.Name}() {{\n" +
                $"      {Constants.MANAGED_FUNCTION_NAME}();\n" +
                $"  }}\n" +
                $"  public void {Constants.MANAGED_FUNCTION_NAME}() {{\n" +
                $"{textsResolvingProperties}" +
                $"  }}\n" +
                $"}}\n";
            return partialClass;
        }

        public static string CreateManagedInjectClass(INamedTypeSymbol classSymbol, InjectedProperty[] injectProperties)
        {
            var textsResolvingProperties = GetTextResolvingProperties(injectProperties, "   ");
            var partialClass =
                $"#nullable enable\n" +
                $"using System;\n" +
                $"using {Constants.INJECT_CONTAINER_NAMESPACE};\n" +
                $"namespace {classSymbol.ContainingNamespace};\n" +
                $"public partial class {classSymbol.Name} {{\n" +
                $"  public void {Constants.MANAGED_FUNCTION_NAME}() {{\n" +
                $"{textsResolvingProperties}" +
                $"  }}\n" +
                $"}}\n";
            return partialClass;
        }

        public static string CreateAutoInjectorClass(INamedTypeSymbol classSymbol, InjectedProperty[] injectProperties)
        {
            var textsResolvingProperties = GetTextResolvingProperties(injectProperties, "   ");
            var partialClass = 
                $"#nullable enable\n" +
                $"using System;\n" +
                $"using {Constants.INJECT_CONTAINER_NAMESPACE};\n" +
                $"namespace {classSymbol.ContainingNamespace};\n" +
                $"public partial class {classSymbol.Name} {{\n" +
                $"  public {classSymbol.Name}() {{\n" +
                $"{textsResolvingProperties}" +
                $"  }}\n" +
                $"}}\n";
            return partialClass;
        }

        private static string GetTextResolvingProperties(InjectedProperty[] injectProperties, string linePrefix = "")
        {
            var textsResolvingProperties = injectProperties.Select(prop => $"{prop.PropertySymbol.Name} = InjectContainer.Resolve<{prop.PropertySymbol.Type}>({(prop.ServiceKey == string.Empty ? "" : $"\"{prop.ServiceKey}\"")});");
            string combinedTextsResolvingProperties = "";
            foreach (var text in textsResolvingProperties)
            {
                combinedTextsResolvingProperties += $"{linePrefix}{text}\n";
            }
            return combinedTextsResolvingProperties;
        }

        private static bool CaptureClassSyntax(SyntaxNode syntax, CancellationToken token)
        {
            return syntax is ClassDeclarationSyntax;
        }
        private static ClassDeclarationSyntax ConvertToClass(GeneratorSyntaxContext context, CancellationToken token)
        {
            return (ClassDeclarationSyntax)context.Node;
        }
    }
}

                