using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;
using System.Linq;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal static class DocumentEditorExtensions
    {
        public static void ReplaceRangeNodes(this SyntaxEditor editor, SelectionResult selectionResult, IEnumerable<SyntaxNode> nodes)
        {
            var firstStatement = selectionResult.FirstStatement();
            var firstNodeToInsert = nodes.First();

            editor.ReplaceNode(firstStatement, firstNodeToInsert);
            foreach (var statementSyntax in selectionResult.OtherThanFirstStatements())
            {
                editor.RemoveNode(statementSyntax);
            }

            var otherNodes = nodes.Skip(1);
            if (!otherNodes.Any())
            {
                return;
            }

            editor.InsertAfter(firstStatement, otherNodes);
        }
    }
}
