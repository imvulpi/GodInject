using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace GodInject.Prebuild.injection_generator.data
{
    public class InjectedDataMembers
    {
        public InjectedDataMembers(INamedTypeSymbol classSymbol)
        {
            InjectedProperties = GetInjectedProperties(classSymbol);
            InjectedFields = GetInjectedFields(classSymbol);
        }

        public InjectedProperty[] InjectedProperties { get; set; }
        public InjectedField[] InjectedFields { get; set; }
        public InjectedProperty[] GetInjectedProperties(INamedTypeSymbol classSymbol)
        {
            var properties = classSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.GetAttributes().Any(attr =>
                {
                    if(attr.AttributeClass == null) return false;
                    return attr.AttributeClass.Name == "Inject";
                }))
                .ToArray();

            var injectedProperties = new List<InjectedProperty>();
            foreach (var property in properties)
            {
                var injectAttribute = property.GetAttributes().FirstOrDefault(attr =>
                {
                    if (attr.AttributeClass == null) return false;
                    return attr.AttributeClass.Name == "Inject";
                });

                if (injectAttribute != null && injectAttribute.ConstructorArguments.Length > 0)
                {
                    var serviceKey = injectAttribute.ConstructorArguments[0].IsNull ? string.Empty : (string?)injectAttribute.ConstructorArguments[0].Value;
                    InjectedProperty injectedProperty = new InjectedProperty(property, serviceKey);
                    injectedProperties.Add(injectedProperty);
                }
                else
                {
                    InjectedProperty injectedProperty = new InjectedProperty(property, null);
                    injectedProperties.Add(injectedProperty);
                }
            }
            return injectedProperties.ToArray();
        }

        public InjectedField[] GetInjectedFields(INamedTypeSymbol classSymbol)
        {
            var fields = classSymbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(p => p.GetAttributes().Any(attr =>
                {
                    if (attr.AttributeClass == null) return false;
                    return attr.AttributeClass.Name == "Inject";
                }))
                .ToArray();

            var injectedFields = new List<InjectedField>();
            foreach (var field in fields)
            {
                var injectAttribute = field.GetAttributes().FirstOrDefault(attr => 
                { 
                    if (attr.AttributeClass == null) return false; 
                    return attr.AttributeClass.Name == "Inject"; 
                });

                if (injectAttribute != null && injectAttribute.ConstructorArguments.Length > 0)
                {
                    var serviceKey = injectAttribute.ConstructorArguments.FirstOrDefault().IsNull ? string.Empty : (string?)injectAttribute.ConstructorArguments.FirstOrDefault().Value;
                    InjectedField injectedField = new InjectedField(field, serviceKey);
                    injectedFields.Add(injectedField);
                }
                else
                {
                    InjectedField injectedField = new InjectedField(field, null);
                    injectedFields.Add(injectedField);
                }
            }
            return injectedFields.ToArray();
        }
    }
}
