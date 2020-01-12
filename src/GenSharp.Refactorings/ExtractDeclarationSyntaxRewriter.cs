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
            var classDeclaration = InsertExtractedMethods(node);
            classDeclaration = ReplaceStatementWithCalls(classDeclaration);
            return classDeclaration;
        }

        private ClassDeclarationSyntax InsertExtractedMethods(ClassDeclarationSyntax node)
        {
            _methodGenerator.Visit(node.SyntaxTree.GetRoot());
            var members = _methodGenerator.ExtractedStatements.Select(model => model.Method).ToArray();
            var classDeclarationSyntax = node.AddMembers(members);
            return classDeclarationSyntax;
        }

        private ClassDeclarationSyntax ReplaceStatementWithCalls(ClassDeclarationSyntax node)
        {
            var nodeWithReplacedStatements = node;
            foreach (var model in _methodGenerator.ExtractedStatements)
            {
                var variableName = model.Statement.DescendantNodes().OfType<VariableDeclaratorSyntax>().Select(s => s.Identifier).Single();
                var variableUses = nodeWithReplacedStatements.DescendantNodes().OfType<IdentifierNameSyntax>().Where(i => i.Identifier.Value.Equals(variableName.Value));
                nodeWithReplacedStatements = (ClassDeclarationSyntax)nodeWithReplacedStatements.SyntaxTree.GetRoot().ReplaceNodes(variableUses, (original, replaced) => model.Call.Expression);
            }
            return nodeWithReplacedStatements;
        }
    }
}
