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

        public List<ExtractedStatementModel> ExtractedStatements { get; }

        public GenerateMethodFromStatementSyntaxWalker(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;

            ExtractedStatements = new List<ExtractedStatementModel>();
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var afterEqualitySign = node.ChildNodes().OfType<VariableDeclaratorSyntax>().Single().Initializer.Value;
            if (afterEqualitySign is LiteralExpressionSyntax)
            {
                base.VisitVariableDeclaration(node);
                return;
            }

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
            var name = node.Variables
                .Where(v => !string.IsNullOrEmpty(v.Identifier.Text))
                .Select(v => v.Identifier.Text)
                .Single();
            var body = node.ChildNodes().OfType<VariableDeclaratorSyntax>().Single().Initializer.Value;
            var parameters = Parameters(body);

            var methodRoot = SyntaxFactory.MethodDeclaration(node.Type, name)
                .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(body)))
                .AddParameterListParameters(parameters.ToArray());

            return methodRoot;
        }

        private List<ParameterSyntax> Parameters(ExpressionSyntax expression)
        {
            var parameters = new List<ParameterSyntax>();

            var identifiers = expression.DescendantTokens().Where(t => t.IsKind(SyntaxKind.IdentifierToken));
            foreach (var identifier in identifiers)
            {
                var symbol = _semanticModel.GetSymbolInfo(identifier.Parent);
                var type = SyntaxFactory.ParseTypeName(symbol.Symbol.ToDisplayString());
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
