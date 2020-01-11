using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace GenSharp.Refactorings
{
    class GenerateMethodFromStatementSyntaxWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;

        public List<(SyntaxNode Node, MethodDeclarationSyntax Method)> NodeMethodPairs { get; private set; }

        public GenerateMethodFromStatementSyntaxWalker(SemanticModel semanticModel) : base(SyntaxWalkerDepth.Node)
        {
            _semanticModel = semanticModel;

            NodeMethodPairs = new List<(SyntaxNode, MethodDeclarationSyntax)>();
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            NodeMethodPairs.Add((node, MethodWithParameters(node)));

            base.VisitVariableDeclaration(node);
        }

        private MethodDeclarationSyntax MethodWithParameters(VariableDeclarationSyntax node)
        {
            var methodName = node.Variables.Where(v => !string.IsNullOrEmpty(v.Identifier.Text)).Select(v => v.Identifier.Text).Single();
            var returnExpression = node.DescendantNodes().OfType<BinaryExpressionSyntax>().Cast<ExpressionSyntax>().Single();
            var parameters = Parameters(returnExpression);

            var methodRoot = SyntaxFactory.MethodDeclaration(node.Type, methodName)
                .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(returnExpression)))
                .AddParameterListParameters(parameters.ToArray())
                .NormalizeWhitespace()
                .WithLeadingTrivia();

            return methodRoot;
        }

        private List<ParameterSyntax> Parameters(ExpressionSyntax returnExpression)
        {
            var parameters = new List<ParameterSyntax>();

            var identifiers = returnExpression.DescendantTokens().Where(t => t.IsKind(SyntaxKind.IdentifierToken));
            foreach (var identifier in identifiers)
            {
                var identifierType = _semanticModel.GetTypeInfo(identifier.Parent).Type;
                var type = SyntaxFactory.ParseTypeName(identifierType.Name);
                var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(identifier.ValueText)).WithType(type);
                parameters.Add(parameter);
            }

            return parameters;
        }
    }
}
