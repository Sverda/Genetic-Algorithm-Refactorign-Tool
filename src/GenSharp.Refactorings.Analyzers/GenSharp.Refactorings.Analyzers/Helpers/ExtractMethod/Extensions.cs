using Microsoft.CodeAnalysis;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal static class Extensions
    {
        public static ITypeSymbol GetLambdaOrAnonymousMethodReturnType(this SemanticModel binding, SyntaxNode node)
        {
            var info = binding.GetSymbolInfo(node);
            if (info.Symbol == null)
            {
                return null;
            }

            var methodSymbol = info.Symbol as IMethodSymbol;
            if (methodSymbol?.MethodKind != MethodKind.AnonymousFunction)
            {
                return null;
            }

            return methodSymbol.ReturnType;
        }
    }
}
