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

        public ExtractDeclarationSyntaxRewriter(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
            _methodGenerator = new GenerateMethodFromStatementSyntaxWalker(_semanticModel);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _methodGenerator.Visit(node.SyntaxTree.GetRoot());

            var classDeclaration = RemoveVariableDeclarations(node);
            classDeclaration = InsertExtractedMethods(classDeclaration);
            classDeclaration = ReplaceVariableCallsWithMethodCalls(classDeclaration);
            return classDeclaration;
        }

        private ClassDeclarationSyntax RemoveVariableDeclarations(ClassDeclarationSyntax node)
        {
            var nodeWithoutDeclarations = node;

            var parentTargetNodes = _methodGenerator.ExtractedStatements.Select(m => m.TargetStatement.Parent);
            nodeWithoutDeclarations = nodeWithoutDeclarations.TrackNodes(parentTargetNodes);
            nodeWithoutDeclarations = RemoveLeadingLine(nodeWithoutDeclarations, nodeWithoutDeclarations.GetCurrentNodes(parentTargetNodes));
            nodeWithoutDeclarations = nodeWithoutDeclarations.RemoveNodes(nodeWithoutDeclarations.GetCurrentNodes(parentTargetNodes), SyntaxRemoveOptions.KeepNoTrivia);

            return nodeWithoutDeclarations;
        }

        private static ClassDeclarationSyntax RemoveLeadingLine(ClassDeclarationSyntax node, IEnumerable<SyntaxNode> parentTargetNodes)
        {
            foreach (var parentTargetNode in parentTargetNodes)
            {
                var nextSibling = GetNextNode(parentTargetNode);
                var emptyLineTrivia = nextSibling.DescendantTrivia().First();
                if (!emptyLineTrivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    continue;
                }

                var siblingWithoutLeadingLine = nextSibling.ReplaceTrivia(emptyLineTrivia, new SyntaxTrivia());
                node = node.ReplaceNode(nextSibling, siblingWithoutLeadingLine);
            }

            return node;
        }

        private static SyntaxNode GetNextNode(SyntaxNode parentTargetNode)
        {
            var childNodes = parentTargetNode.Parent.ChildNodes().ToList();
            return childNodes
                .Zip(childNodes.Skip(1), (c, n) => (Current: c, Next: n))
                .Last(t => t.Current == parentTargetNode).Next;
        }

        private ClassDeclarationSyntax InsertExtractedMethods(ClassDeclarationSyntax node)
        {
            var members = _methodGenerator.ExtractedStatements.Select(model => model.Method).ToArray();
            var classDeclarationSyntax = node.AddMembers(members);
            return classDeclarationSyntax;
        }

        private ClassDeclarationSyntax ReplaceVariableCallsWithMethodCalls(ClassDeclarationSyntax node)
        {
            var nodeWithReplacedVariables = node;
            foreach (var model in _methodGenerator.ExtractedStatements)
            {
                var variableName = model.TargetStatement.DescendantNodes().OfType<VariableDeclaratorSyntax>().Select(s => s.Identifier).Single();
                var variableUses = nodeWithReplacedVariables.DescendantNodes().OfType<IdentifierNameSyntax>().Where(i => i.Identifier.Value.Equals(variableName.Value));
                nodeWithReplacedVariables = nodeWithReplacedVariables.ReplaceNodes(variableUses, (original, replaced) => model.Call.Expression);
            }
            return nodeWithReplacedVariables;
        }
    }
}
