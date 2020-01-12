using Microsoft.CodeAnalysis;
using System.Linq;

namespace GenSharp.Refactorings
{
    public static class SyntaxNodeExtensions
    {
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
