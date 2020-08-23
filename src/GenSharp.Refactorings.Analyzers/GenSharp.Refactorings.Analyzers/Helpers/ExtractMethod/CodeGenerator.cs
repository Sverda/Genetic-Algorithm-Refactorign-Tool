using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal class CodeGenerator
    {
        private readonly SemanticDocument _semanticDocument;
        private readonly AnalyzerResult _analyzerResult;

        public CodeGenerator(SemanticDocument semanticDocument, AnalyzerResult analyzerResult)
        {
            _semanticDocument = semanticDocument;
            _analyzerResult = analyzerResult;
        }

        public MethodDeclarationSyntax ConstructMethodDeclaration(MethodDeclarationSyntax extractFrom, SelectionResult selectionResult)
        {
            var body = selectionResult.AsBody();
            body = AppendReturnStatementIfNeeded(body);

            var extractedMethod = SyntaxFactory
                .MethodDeclaration(SyntaxFactory.ParseTypeName("void"), $"{extractFrom.Identifier.Text}_ExtractedMethod")
                .AddParameterListParameters(CreateMethodParameters(_analyzerResult).ToArray())
                .WithBody(body)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
            return extractedMethod;
        }

        private BlockSyntax AppendReturnStatementIfNeeded(BlockSyntax body)
        {
            if (!_analyzerResult.HasVariableToUseAsReturnValue)
            {
                return body;
            }

            var statements = body.Statements;
            var returnStatement = CreateReturnStatement(_analyzerResult.VariableToUseAsReturnValue.Name);
            var statementSyntaxes = statements.Concat(new []{ returnStatement });
            return SyntaxFactory.Block(statementSyntaxes);
        }

        private StatementSyntax CreateReturnStatement(string identifierName = null)
        {
            return string.IsNullOrEmpty(identifierName)
                ? SyntaxFactory.ReturnStatement()
                : SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(identifierName));
        }

        private static List<ParameterSyntax> CreateMethodParameters(AnalyzerResult analyzerResult)
        {
            var parameters = new List<ParameterSyntax>();

            foreach (var methodParameter in analyzerResult.MethodParameters)
            {
                var typeSyntax = SyntaxFactory.ParseTypeName(methodParameter.Type.ToDisplayString());
                var parameter = SyntaxFactory
                    .Parameter(SyntaxFactory.Identifier(methodParameter.Name))
                    .WithType(typeSyntax);
                parameters.Add(parameter);
            }

            return parameters;
        }

        public ExpressionStatementSyntax CreateCallSignature(MethodDeclarationSyntax method)
        {
            var methodName = SyntaxFactory.ParseExpression(method.Identifier.ValueText);

            var arguments = new List<ArgumentSyntax>();
            foreach (var argument in _analyzerResult.MethodParameters)
            {
                var modifier = GetParameterRefSyntaxKind(argument.ParameterModifier);
                var refOrOut = modifier == SyntaxKind.None ? default : SyntaxFactory.Token(modifier);

                arguments.Add(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(argument.Name)).WithRefOrOutKeyword(refOrOut));
            }

            var argumentListSyntax = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
            var invocation = SyntaxFactory.InvocationExpression(methodName, argumentListSyntax);

            return SyntaxFactory
                .ExpressionStatement(invocation)
                .NormalizeWhitespace()
                .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
        }

        private static SyntaxKind GetParameterRefSyntaxKind(ParameterBehavior parameterBehavior)
        {
            var parameterRefSyntaxKind = parameterBehavior == ParameterBehavior.Out ? SyntaxKind.OutKeyword : SyntaxKind.None;
            return parameterBehavior == ParameterBehavior.Ref ? SyntaxKind.RefKeyword : parameterRefSyntaxKind;
        }
    }
}
