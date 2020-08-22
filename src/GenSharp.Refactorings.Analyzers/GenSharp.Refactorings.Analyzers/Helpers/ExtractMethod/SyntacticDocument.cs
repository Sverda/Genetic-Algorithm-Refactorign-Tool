using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal class SyntacticDocument
    {
        public readonly Document Document;
        public readonly SourceText Text;
        public readonly SyntaxTree SyntaxTree;
        public readonly SyntaxNode Root;

        protected SyntacticDocument(Document document, SourceText text, SyntaxTree tree, SyntaxNode root)
        {
            Document = document;
            Text = text;
            SyntaxTree = tree;
            Root = root;
        }
    }
}
