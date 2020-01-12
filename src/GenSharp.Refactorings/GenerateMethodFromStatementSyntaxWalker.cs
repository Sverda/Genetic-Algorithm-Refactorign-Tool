using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GenSharp.Refactorings
{
    class GenerateMethodFromStatementSyntaxWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;

        public List<ExtractedStatementModel> ExtractedStatements { get; private set; }

        public GenerateMethodFromStatementSyntaxWalker(SemanticModel semanticModel) : base(SyntaxWalkerDepth.Node)
        {
            _semanticModel = semanticModel;

            ExtractedStatements = new List<ExtractedStatementModel>();
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var model = new ExtractedStatementModel();
            model.Statement = node;
            model.Method = MethodWithParameters(node);
            model.Call = Call(model.Method);
            ExtractedStatements.Add(model);

            base.VisitVariableDeclaration(node);
        }

        //TODO: Move it to other class
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

        private ExpressionStatementSyntax Call(MethodDeclarationSyntax method)
        {
            var className = GetClassIdentifier();
            var methodName = SyntaxFactory.IdentifierName(method.Identifier);
            var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, className, methodName);

            //TODO: Parse parameters to arguments
            var argument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("A")));
            var argumentList = SyntaxFactory.SeparatedList(new[] { argument });

            var methodCall = SyntaxFactory.ExpressionStatement
            (
                SyntaxFactory.InvocationExpression(memberAccess, SyntaxFactory.ArgumentList(argumentList))
            );

            Trace.WriteLine(methodCall.ToFullString());

            return methodCall;
        }

        private static IdentifierNameSyntax GetClassIdentifier()
        {
            return SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("this"));
        }
    }
}
