using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace GenSharp.Refactorings
{
    public class ExtractDeclarationSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;

        private readonly GenerateMethodFromStatementSyntaxWalker _methodGenerator;

        private ClassDeclarationSyntax _currentClassNode;

        public ExtractDeclarationSyntaxRewriter(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
            _methodGenerator = new GenerateMethodFromStatementSyntaxWalker(_semanticModel);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _currentClassNode = node;
            _methodGenerator.Visit(_currentClassNode.SyntaxTree.GetRoot());

            RemoveVariableDeclarations();
            InsertExtractedMethods();
            ReplaceVariableCallsWithMethodCalls();
            return _currentClassNode;
        }

        private void RemoveVariableDeclarations()
        {
            var parentTargetNodes = _methodGenerator.ExtractedStatements.Select(m => m.TargetStatement.Parent);
            _currentClassNode = _currentClassNode.TrackNodes(parentTargetNodes);
            RemoveLeadingLine(_currentClassNode.GetCurrentNodes(parentTargetNodes));
            _currentClassNode = _currentClassNode.RemoveNodes(_currentClassNode.GetCurrentNodes(parentTargetNodes), SyntaxRemoveOptions.KeepNoTrivia);
        }

        private void RemoveLeadingLine(IEnumerable<SyntaxNode> parentTargetNodes)
        {
            foreach (var parentTargetNode in parentTargetNodes)
            {
                var nextSibling = parentTargetNode.GetNextNode();
                var emptyLineTrivia = nextSibling.DescendantTrivia().First();
                if (!emptyLineTrivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    continue;
                }

                var siblingWithoutLeadingLine = nextSibling.ReplaceTrivia(emptyLineTrivia, new SyntaxTrivia());
                _currentClassNode = _currentClassNode.ReplaceNode(nextSibling, siblingWithoutLeadingLine);
            }
        }

        private void InsertExtractedMethods()
        {
            var members = _methodGenerator.ExtractedStatements.Select(model => model.Method).ToArray();
            _currentClassNode = _currentClassNode.AddMembers(members);
        }

        private void ReplaceVariableCallsWithMethodCalls()
        {
            foreach (var model in _methodGenerator.ExtractedStatements)
            {
                var variableName = model.TargetStatement.DescendantNodes().OfType<VariableDeclaratorSyntax>().Select(s => s.Identifier).Single();
                var variableUses = _currentClassNode.DescendantNodes().OfType<IdentifierNameSyntax>().Where(i => i.Identifier.Value.Equals(variableName.Value));
                _currentClassNode = _currentClassNode.ReplaceNodes(variableUses, (original, replaced) => model.Call.Expression);
            }
        }
    }
}
