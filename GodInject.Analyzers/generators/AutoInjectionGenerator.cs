using GodInject.Analyzers.generators.builder;
using GodInject.Analyzers.generators.data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Threading;

namespace GodInject.Analyzers.generators
{
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

            #if DEBUG
            debugs.DebugOutputter debugOutputter = new debugs.DebugOutputter();
            debugOutputter.Initialize(context, compilationAndClasses);
            #endif

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

                        InjectedDataMembers injectedDataMembers = new InjectedDataMembers();
                        injectedDataMembers.InjectedFields = injectedDataMembers.GetInjectedFields(classSymbol, injectAttributeSymbol);
                        injectedDataMembers.InjectedProperties = injectedDataMembers.GetInjectedProperties(classSymbol, injectAttributeSymbol);
                        InjectClassBuilder injectClassBuilder = new InjectClassBuilder();                        
                        if (injectedDataMembers.InjectedProperties.Length > 0 || injectedDataMembers.InjectedFields.Length > 0)
                        {
                            var managedInjectAttributeSymbol = compilation.GetTypeByMetadataName(Constants.MANAGED_INJECT_ATTRIBUTE_NAMESPACE);
                            bool isManagedInject = classSymbol.GetAttributes().Any(attr => SymbolEqualityComparer.IncludeNullability.Equals(attr.AttributeClass, managedInjectAttributeSymbol));
                            var attribute = classSymbol.GetAttributes().FirstOrDefault(attr => SymbolEqualityComparer.IncludeNullability.Equals(attr.AttributeClass, managedInjectAttributeSymbol));
                            if (isManagedInject)
                            {
                                if (attribute != null && (bool)attribute.ConstructorArguments[0].Value == true) // Allow parameterless
                                {
                                    spc.AddSource($"{classSymbol.Name}.g.cs", SourceText.From(injectClassBuilder.CreateClass(classSymbol, injectedDataMembers, true, true), Encoding.Unicode));
                                }
                                else
                                {
                                    spc.AddSource($"{classSymbol.Name}.g.cs", SourceText.From(injectClassBuilder.CreateClass(classSymbol, injectedDataMembers, true, false), Encoding.Unicode));
                                }
                            }
                            else
                            {
                                //// On default auto injection through a parameterless constructor
                                spc.AddSource($"{classSymbol.Name}.g.cs", SourceText.From(injectClassBuilder.CreateClass(classSymbol, injectedDataMembers, false, true), Encoding.Unicode));
                            }
                        }
                    }
                }
            });
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

                