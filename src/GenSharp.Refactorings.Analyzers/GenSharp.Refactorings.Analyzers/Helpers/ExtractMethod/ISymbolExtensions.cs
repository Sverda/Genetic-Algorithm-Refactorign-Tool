using Microsoft.CodeAnalysis;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal static class ISymbolExtensions
    {
        public static bool IsNormalAnonymousType(this ISymbol symbol)
            => symbol.IsAnonymousType() && !symbol.IsDelegateType();

        public static bool IsAnonymousDelegateType(this ISymbol symbol)
            => symbol.IsAnonymousType() && symbol.IsDelegateType();

        public static bool IsDelegateType(this ISymbol symbol)
            => symbol is ITypeSymbol typeSymbol && typeSymbol.TypeKind == TypeKind.Delegate;

        public static bool IsAnonymousType(this ISymbol symbol)
            => symbol is INamedTypeSymbol typeSymbol && typeSymbol.IsAnonymousType;

        public static bool IsPointerType(this ISymbol symbol)
            => symbol is IPointerTypeSymbol;

        public static bool IsEnumType(this ITypeSymbol type)
            => IsEnumType(type, out _);

        public static bool IsEnumType(this ITypeSymbol type, out INamedTypeSymbol enumType)
        {
            if (type != null && type.IsValueType && type.TypeKind == TypeKind.Enum)
            {
                enumType = (INamedTypeSymbol)type;
                return true;
            }

            enumType = null;
            return false;
        }
    }
}
