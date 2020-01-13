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

        public List<ExtractedStatementModel> ExtractedStatements { get; private set; }

        public GenerateMethodFromStatementSyntaxWalker(SemanticModel semanticModel) : base(SyntaxWalkerDepth.Node)
        {
            _semanticModel = semanticModel;

            ExtractedStatements = new List<ExtractedStatementModel>();
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var model = new ExtractedStatementModel();
            model.TargetStatement = node;
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
                .AddParameterListParameters(parameters.ToArray());

            return methodRoot;
        }

        private List<ParameterSyntax> Parameters(ExpressionSyntax expression)
        {
            var parameters = new List<ParameterSyntax>();

            var identifiers = expression.DescendantTokens().Where(t => t.IsKind(SyntaxKind.IdentifierToken));
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
            var argumentsList = Arguments(method);

            var methodAccess = SyntaxFactory.ParseExpression(method.Identifier.ValueText);

            var methodCall = SyntaxFactory.ExpressionStatement
            (
                SyntaxFactory.InvocationExpression(methodAccess, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(argumentsList)))
            );

            return methodCall.NormalizeWhitespace();
        }

        private static List<ArgumentSyntax> Arguments(MethodDeclarationSyntax method)
        {
            var argumentsList = new List<ArgumentSyntax>();
            foreach (var parameter in method.ParameterList.Parameters)
            {
                var expression = SyntaxFactory.ParseExpression(parameter.Identifier.Text);
                var argument = SyntaxFactory.Argument(expression);
                argumentsList.Add(argument);
            }

            return argumentsList;
        }
    }
}
