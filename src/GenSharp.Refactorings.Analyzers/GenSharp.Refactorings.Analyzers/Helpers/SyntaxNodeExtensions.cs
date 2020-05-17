using Microsoft.CodeAnalysis;
using System.Linq;

namespace GenSharp.Refactorings.Analyzers.Helpers
{
    public static class SyntaxNodeExtensions
    {
        public static SyntaxNode GetPreviousNode(this SyntaxNode node)
        {
            var childNodes = node.Parent.ChildNodes().ToList();
            var sibling = childNodes
                .Zip(childNodes.Skip(1), (p, c) => (Previous: p, Current: c))
                .Last(t => t.Current == node).Previous;
            return sibling;
        }

        public static SyntaxNode GetNextNode(this SyntaxNode node)
        {
            var childNodes = node.Parent.ChildNodes().ToList();
            var sibling = childNodes
                .Zip(childNodes.Skip(1), (c, n) => (Current: c, Next: n))
                .Last(t => t.Current == node).Next;
            return sibling;
        }
    }
}
