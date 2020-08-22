using Microsoft.CodeAnalysis;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal static class ITypeSymbolExtensions
    {
        public static bool ContainsAnonymousType(this ITypeSymbol symbol)
        {
            switch (symbol)
            {
                case IArrayTypeSymbol a: return ContainsAnonymousType(a.ElementType);
                case IPointerTypeSymbol p: return ContainsAnonymousType(p.PointedAtType);
                case INamedTypeSymbol n: return ContainsAnonymousType(n);
                default: return false;
            }
        }

        private static bool ContainsAnonymousType(INamedTypeSymbol type)
        {
            if (type.IsAnonymousType)
            {
                return true;
            }

            foreach (var typeArg in type.GetAllTypeArguments())
            {
                if (ContainsAnonymousType(typeArg))
                {
                    return true;
                }
            }

            return false;
        }

        public static ITypeSymbol RemoveAnonymousTypes(this ITypeSymbol type, Compilation compilation)
        {
            return type?.Accept(new AnonymousTypeRemover(compilation));
        }
    }
}
