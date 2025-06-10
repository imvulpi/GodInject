using GodInject.Prebuild.generators.data;
using System.Linq;

namespace GodInject.Prebuild.generators.builder
{
    public class InjectResolverBuilder
    {
        public string GetTextResolvingDataMembers(InjectedDataMembers dataMembers)
        {
            string text = BuilderHelper.FormatNewLines(
                GetTextsResolvingMembers(dataMembers.InjectedFields),
                GetTextResolvingMembers(dataMembers.InjectedProperties));
            return text;
        }

        public string GetTextResolvingMembers(InjectedProperty[] injectProperties)
        {
            var textsResolvingProperties = injectProperties.Select(GetTextResolvingMembers);
            return BuilderHelper.FormatNewLines(textsResolvingProperties.ToArray());

        }

        public string GetTextsResolvingMembers(InjectedField[] injectFields)
        {
            var textsResolvingFields = injectFields.Select(GetTextResolvingMember);
            return BuilderHelper.FormatNewLines(textsResolvingFields.ToArray());
        }

        private string GetTextResolvingMember(InjectedField field)
        {
            string resolveFunc;
            if (field.ServiceKey == null)
                resolveFunc = $"{Constants.CONTAINER_CLASS_NAME}.Resolve<{field.PropertySymbol.Type}>();";
            else
                resolveFunc = $"{Constants.CONTAINER_CLASS_NAME}.Resolve<{field.PropertySymbol.Type}>(\"{field.ServiceKey}\");";

            return $"{field.PropertySymbol.Name} = {resolveFunc}";
        }

        private string GetTextResolvingMembers(InjectedProperty property)
        {
            string resolveFunc;
            if (property.ServiceKey == null)
                resolveFunc = $"{Constants.CONTAINER_CLASS_NAME}.Resolve<{property.PropertySymbol.Type}>();";
            else
                resolveFunc = $"{Constants.CONTAINER_CLASS_NAME}.Resolve<{property.PropertySymbol.Type}>(\"{property.ServiceKey}\");";

            return $"{property.PropertySymbol.Name} = {resolveFunc}";
        }
    }
}
