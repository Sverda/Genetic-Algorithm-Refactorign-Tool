using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

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
            var extractedMethod = SyntaxFactory
                .MethodDeclaration(SyntaxFactory.ParseTypeName("void"), $"{extractFrom.Identifier.Text}_ExtractedMethod")
                .AddParameterListParameters(CreateMethodParameters(_analyzerResult).ToArray())
                .WithBody(selectionResult.AsBody())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
            return extractedMethod;
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
