using Microsoft.CodeAnalysis;
using System.Text;

namespace GodInject.Prebuild.generators.builder
{
    public static class BuilderHelper
    {
        public static string FormatNewLines(params string[] texts)
        {
            if (texts.Length == 0) return "";
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < texts.Length-1; i++)
            {
                if (texts[i] == null) continue;
                stringBuilder.AppendLine(texts[i]);
            }
            stringBuilder.Append(texts[texts.Length-1]);
            return stringBuilder.ToString();
        }

        public static bool HasMethod(INamedTypeSymbol typeSymbol, string methodName)
        {
            for (var current = typeSymbol; current != null; current = current.BaseType)
            {
                var hasMethod = current
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => {
                    if (m.Name == methodName)
                        {
                            return true;
                        }
                        return false;
                    });

                if (hasMethod)
                    return true;
            }

            foreach (var interfaceType in typeSymbol.AllInterfaces)
            {
                var hasMethod = interfaceType
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => {
                        if (m.Name == methodName)
                        {
                            return true;
                        }
                        return false;
                    });
                if (hasMethod)
                    return true;
            }

            return false;

        }

        public static bool HasNotOverridenVirtualMethod(INamedTypeSymbol typeSymbol, string methodName)
        {
            bool isOverriden = typeSymbol.GetMembers().OfType<IMethodSymbol>().Any(m => m.Name == methodName && m.OverriddenMethod != null);
            if (isOverriden) return false;
            for (var current = typeSymbol; current != null; current = current.BaseType)
            {
                var hasMethod = current
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => {
                        if (m.Name == methodName) {
                            return m.IsVirtual;
                        }
                        return false;
                    });

                if (hasMethod)
                    return true;
            }

            foreach (var interfaceType in typeSymbol.AllInterfaces)
            {
                var hasMethod = interfaceType
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => m.Name == methodName && m.IsVirtual);

                if (hasMethod)
                    return true;
            }

            return false;
        }
    }
}
