using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal class CodeGenerator
    {
        private readonly SemanticDocument _semanticDocument;
        private readonly AnalyzerResult _analyzerResult;
        private readonly SelectionResult _selectionResult;

        public CodeGenerator(SemanticDocument semanticDocument, AnalyzerResult analyzerResult, SelectionResult selectionResult)
        {
            _semanticDocument = semanticDocument;
            _analyzerResult = analyzerResult;
            _selectionResult = selectionResult;
        }

        public MethodDeclarationSyntax GenerateMethodDefinition(MethodDeclarationSyntax extractFrom, CancellationToken cancellationToken)
        {
            var body = _selectionResult.AsBody();
            body = AppendReturnStatementIfNeeded(body);
            body = SplitOrMoveDeclarationIntoMethodDefinition(body, cancellationToken);

            var displayName = _analyzerResult.ReturnType.ToMinimalDisplayString(_semanticDocument.SemanticModel, 0);
            var returnType = SyntaxFactory.ParseTypeName(displayName);

            var extractedMethod = SyntaxFactory
                .MethodDeclaration(returnType,
                    $"{extractFrom.Identifier.Text}_ExtractedMethod")
                .AddParameterListParameters(CreateMethodParameters().ToArray())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .WithBody(body);
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
            var statementSyntaxes = statements.Concat(new[] { returnStatement });
            return SyntaxFactory.Block(statementSyntaxes);
        }

        private static StatementSyntax CreateReturnStatement(string identifierName = null)
        {
            return string.IsNullOrEmpty(identifierName)
                ? SyntaxFactory.ReturnStatement()
                : SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(identifierName));
        }

        private BlockSyntax SplitOrMoveDeclarationIntoMethodDefinition(BlockSyntax body, CancellationToken cancellationToken)
        {
            var statements = body.Statements;
            var postProcessor = new PostProcessor(_semanticDocument.SemanticModel, _selectionResult.FirstStatement().SpanStart);

            var variables = _analyzerResult.GetVariablesToSplitOrMoveIntoMethodDefinition(cancellationToken);
            var declarationStatements = CreateDeclarationStatements(variables, cancellationToken);
            declarationStatements = postProcessor.MergeDeclarationStatements(declarationStatements);

            return SyntaxFactory.Block(declarationStatements.Concat(statements));
        }

        private IEnumerable<StatementSyntax> CreateDeclarationStatements(IEnumerable<VariableInfo> variables, CancellationToken cancellationToken)
        {
            var list = new List<StatementSyntax>();

            foreach (var variable in variables)
            {
                var declaration = CreateDeclarationStatement(variable, initialValue: null);
                list.Add(declaration);
            }

            return list;
        }

        private List<ParameterSyntax> CreateMethodParameters()
        {
            var parameters = new List<ParameterSyntax>();

            foreach (var methodParameter in _analyzerResult.MethodParameters)
            {
                var typeSyntax = SyntaxFactory.ParseTypeName(methodParameter.Type.ToDisplayString());
                var parameter = SyntaxFactory
                    .Parameter(SyntaxFactory.Identifier(methodParameter.Name))
                    .WithType(typeSyntax);
                parameters.Add(parameter);
            }

            return parameters;
        }

        private StatementSyntax CreateDeclarationStatement(
            VariableInfo variable,
            ExpressionSyntax initialValue)
        {
            var typeNode =
                SyntaxFactory.ParseTypeName(variable.Type.ToMinimalDisplayString(_semanticDocument.SemanticModel, 0));

            var equalsValueClause = initialValue == null ? null : SyntaxFactory.EqualsValueClause(value: initialValue);

            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(typeNode)
                    .AddVariables(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(variable.Name)).WithInitializer(equalsValueClause)));
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
