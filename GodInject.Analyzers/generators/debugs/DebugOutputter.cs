#if DEBUG
using GodInject.Analyzers.generators.data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;

namespace GodInject.Analyzers.generators.debugs
{
    /// <summary>
    /// Handles generation of DEBUG Outputs - when <see cref="Constants.OPTION_DEBUG"/> is turned on.
    /// </summary>
    public class DebugOutputter
    {
        public void Initialize(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses)
        {
            var debugEnabledData = context.AnalyzerConfigOptionsProvider
            .Select((options, _) =>
            {
                if (options.GlobalOptions.TryGetValue($"build_property.{Constants.OPTION_DEBUG}", out var configPath))
                {
                    return true;
                }
                return false;
            });

            var debugData = debugEnabledData.Combine(compilationAndClasses);
            context.RegisterSourceOutput(debugData, (spc, source) =>
            {
                var (debugEnabled, (compilation, classList)) = source;
                if (debugEnabled)
                {
                    spc.AddSource($"AIGConfiguration-{DateTime.Now:yyyyMMddHHmmss}.g.cs", GetConfigurationText());
                    var injectAttributeSymbol = compilation.GetTypeByMetadataName(Constants.INJECT_ATTRIBUTE_NAMESPACE);
                    var summary = GetSummaryStart();
                    summary += injectAttributeSymbol == null ? $"\n{Constants.INJECT_ATTRIBUTE_NAMESPACE} not found!" : $"\n{Constants.INJECT_ATTRIBUTE_NAMESPACE} found!";
                    summary += "\n---------------------------------------------------------------\n";
                    foreach (var classSyntax in classList)
                    {
                        var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
                        if (semanticModel.GetDeclaredSymbol(classSyntax) is INamedTypeSymbol classSymbol)
                        {
                            var className = classSymbol.Name;
                            InjectedDataMembers injectedDataMembers = new InjectedDataMembers();
                            injectedDataMembers.InjectedFields = injectedDataMembers.GetInjectedFields(classSymbol, injectAttributeSymbol);
                            injectedDataMembers.InjectedProperties = injectedDataMembers.GetInjectedProperties(classSymbol, injectAttributeSymbol);
                            summary += $"\n[{className}]";
                            AddMembersDebugTexts(ref summary, injectedDataMembers);
                        }
                    }
                    ;
                    summary += "\n\n*/";
                    spc.AddSource($"AIGSummary-{DateTime.Now:yyyyMMddHHmmss}.g.cs", summary);
                }
            });
        }

        private void AddMembersDebugTexts(ref string summary, InjectedDataMembers injectedDataMembers)
        {
            if (injectedDataMembers.InjectedProperties.Length > 0)
            {
                summary += $"\n\t{$"{injectedDataMembers.InjectedProperties.Length} Properties with {Constants.INJECT_ATTRIBUTE_NAMESPACE}"}";
                for (int i = 0; i < injectedDataMembers.InjectedProperties.Length; i++)
                {
                    var property = injectedDataMembers.InjectedProperties[i];
                    summary += $"\n\t\t{i}. - {property.PropertySymbol.Name} | Service key: {(property.ServiceKey ?? "Not used")}";
                }
            }
            else
            {
                summary += $"\n\t{Constants.INJECT_ATTRIBUTE_NAMESPACE}";
            }

            if (injectedDataMembers.InjectedFields.Length > 0)
            {
                summary += $"\n\t{$"{injectedDataMembers.InjectedFields.Length} Fields with {Constants.INJECT_ATTRIBUTE_NAMESPACE}"}";
                for (int i = 0; i < injectedDataMembers.InjectedFields.Length; i++)
                {
                    var field = injectedDataMembers.InjectedFields[i];
                    summary += $"\n\t\t{i}. - {field.PropertySymbol.Name} | Service key: {(field.ServiceKey ?? "Not used")}";
                }
            }
            else
            {
                summary += $"\n\t{Constants.INJECT_ATTRIBUTE_NAMESPACE}";
            }
        }

        public string GetSummaryStart()
        {
            return $"/*\n" +
                $"---------------------------------------------------------------\n" +
                $"GENERATED Summary of all classes\n" +
                $"---------------------------------------------------------------";
        }

        public string GetConfigurationText()
        {
            return $"/*\n" +
                $"---------------------------------------------------------------\n" +
                $"GENERATED configuration file for {nameof(AutoInjectionGenerator)}\n" +
                $"All options and their values are listed here\n" +
                $"This generated because you have {Constants.OPTION_DEBUG} option\n" +
                $"---------------------------------------------------------------\n" +
                $"\n" +
                $"{Constants.OPTION_DEBUG} - ENABLED\n" +
                $"\n" +
                $"---------------------------------------------------------------\n" +
                $"Add new options via: <CompilerVisibleProperty Include=\"Option\" />\n" +
                $"This version includes options:\n" +
                $"  {Constants.OPTION_DEBUG}\n" +
                $"---------------------------------------------------------------" +
                $"\n*/";
        }
    }
}
#endif