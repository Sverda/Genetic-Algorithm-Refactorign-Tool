using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

            var classDeclaration = RemoveVariableDeclaration(node);
            classDeclaration = InsertExtractedMethods(classDeclaration);
            classDeclaration = ReplaceVariableCallsWithMethodCalls(classDeclaration);
            return classDeclaration;
        }

        private ClassDeclarationSyntax RemoveVariableDeclaration(ClassDeclarationSyntax node)
        {
            var nodeWithoutStatements = node;
            var nodes = _methodGenerator.ExtractedStatements.Select(m => m.TargetStatement.Parent);
            nodeWithoutStatements = nodeWithoutStatements.RemoveNodes(nodes, SyntaxRemoveOptions.KeepNoTrivia);
            return nodeWithoutStatements;
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
