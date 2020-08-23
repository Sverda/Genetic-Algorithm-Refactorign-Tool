using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal static class SyntaxListExtensions
    {
        public static SyntaxList<T> ToSyntaxList<T>(this IEnumerable<T> sequence) where T : SyntaxNode
            => SyntaxFactory.List(sequence);
    }
}
