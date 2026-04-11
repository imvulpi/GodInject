using Microsoft.CodeAnalysis;

namespace GodInject.Generator.utils
{
    /// <summary>
    /// Class providing helpful methods for code symbol processing
    /// </summary>
    public static class SymbolHelper
    {

        /// <summary>
        /// Checks whether a symbol contains a method
        /// </summary>
        /// <param name="symbol">Symbol to check</param>
        /// <param name="methodName">Method name to check</param>
        /// <returns>Whether a method with such name exists</returns>
        public static bool HasMethod(INamedTypeSymbol symbol, string methodName)
        {
            var hasMethod = symbol
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

            return false;
        }

        /// <summary>
        /// Checks whether a symbol has an access to a method (can invoke it)
        /// It checks the specific symbol, base symbols and interfaces.
        /// </summary>
        /// <param name="symbol">Symbol to check</param>
        /// <param name="methodName">Method name to check</param>
        /// <returns>Whether the symbol has access to a method</returns>
        public static bool HasMethodAccess(INamedTypeSymbol symbol, string methodName)
        {
            for (var current = symbol; current != null; current = current.BaseType)
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

            foreach (var interfaceType in symbol.AllInterfaces)
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

        /// <summary>
        /// Checks whether a symbol contains a not overriden virtual method, not overriden meaning the method is not overriden in the
        /// provided symbol, it can be overriden in base classes.
        /// It checks the specific symbol, base symbols and interfaces.
        /// </summary>
        /// <param name="typeSymbol">Symbol to check</param>
        /// <param name="methodName">Method name to check</param>
        /// <returns>Whether a not overriden virtual method exists</returns>
        public static bool HasNotOverridenVirtualMethodAccess(INamedTypeSymbol typeSymbol, string methodName)
        {
            bool isOverriden = typeSymbol.GetMembers().OfType<IMethodSymbol>().Any(m => m.Name == methodName && m.OverriddenMethod != null);
            if (isOverriden) return false;

            for (var current = typeSymbol; current != null; current = current.BaseType)
            {
                var hasMethod = current
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => m.Name == methodName && m.IsVirtual);

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
