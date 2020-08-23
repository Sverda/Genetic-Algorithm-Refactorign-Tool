using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public static bool HasSyntaxAnnotation(this HashSet<SyntaxAnnotation> set, SyntaxNode node)
            => set.Any(a => node.GetAnnotatedNodesAndTokens(a).Any());

        public static Task<SemanticDocument> WithSyntaxRootAsync(this SemanticDocument semanticDocument, SyntaxNode root, CancellationToken cancellationToken)
            => SemanticDocument.CreateAsync(semanticDocument.Document.WithSyntaxRoot(root), cancellationToken);
    }
}
