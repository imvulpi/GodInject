using GodInject.Generator.generator.data;
using GodInject.Generator.utils;

namespace GodInject.Generator.generator
{
    public class InjectResolverBuilder
    {
        public string GetTextResolvingDataMembers(InjectedDataMembers dataMembers)
        {
            string text = TextHelper.FormatNewLines(
                GetTextsResolvingMembers(dataMembers.InjectedFields),
                GetTextResolvingMembers(dataMembers.InjectedProperties));
            return text;
        }

        public string GetTextResolvingMembers(InjectedProperty[] injectProperties)
        {
            var textsResolvingProperties = injectProperties.Select(GetTextResolvingMembers);
            return TextHelper.FormatNewLines(textsResolvingProperties.ToArray());

        }

        public string GetTextsResolvingMembers(InjectedField[] injectFields)
        {
            var textsResolvingFields = injectFields.Select(GetTextResolvingMember);
            return TextHelper.FormatNewLines(textsResolvingFields.ToArray());
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
