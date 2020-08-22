using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal static class DocumentEditorExtensions
    {
        public static void ReplaceRangeNodes(this SyntaxEditor editor, SelectionResult selectionResult, SyntaxNode node)
        {
            editor.ReplaceNode(selectionResult.FirstStatement(), node);
            foreach (var statementSyntax in selectionResult.OtherThanFirstStatements())
            {
                editor.RemoveNode(statementSyntax);
            }
        }
    }
}
