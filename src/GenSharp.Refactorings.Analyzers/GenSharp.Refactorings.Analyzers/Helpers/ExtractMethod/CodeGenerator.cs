using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal class CodeGenerator
    {
        private readonly SemanticDocument _semanticDocument;
        private readonly AnalyzerResult _analyzerResult;
        private readonly SelectionResult _selectionResult;
        private readonly string _methodName;

        public CodeGenerator(SemanticDocument semanticDocument, AnalyzerResult analyzerResult, SelectionResult selectionResult, string methodName)
        {
            _semanticDocument = semanticDocument;
            _analyzerResult = analyzerResult;
            _selectionResult = selectionResult;
            _methodName = methodName;
        }

        public async Task<SemanticDocument> GenerateAsync(CancellationToken cancellationToken)
        {
            // Call Site Method Replacement
            var root = _semanticDocument.Root;
            root = root.ReplaceNode(_selectionResult.GetContainingScope(), await GenerateBodyForCallSiteContainerAsync(cancellationToken).ConfigureAwait(false));
            var callSiteDocument = await _semanticDocument.WithSyntaxRootAsync(root, cancellationToken).ConfigureAwait(false);

            // New Method Insertion
            root = callSiteDocument.Root;
            var destination = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var extractedMethod = GenerateMethodDefinition(cancellationToken);
            var newDestination = destination.AddMembers(extractedMethod);
            root = root.ReplaceNode(destination, newDestination);
            var newMethodDocument =
                await callSiteDocument.WithSyntaxRootAsync(root, cancellationToken).ConfigureAwait(false);

            return newMethodDocument;
        }

        public MethodDeclarationSyntax GenerateMethodDefinition(CancellationToken cancellationToken)
        {
            var body = _selectionResult.AsBody();
            body = SplitOrMoveDeclarationIntoMethodDefinition(body, cancellationToken);
            body = AppendReturnStatementIfNeeded(body);
            body = CleanupCode(body);

            var displayName = _analyzerResult.ReturnType.ToMinimalDisplayString(_semanticDocument.SemanticModel, 0);
            var returnType = SyntaxFactory.ParseTypeName(displayName);

            var extractedMethod = SyntaxFactory
                .MethodDeclaration(returnType, _methodName)
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

        private BlockSyntax CleanupCode(BlockSyntax body)
        {
            var semanticModel = _semanticDocument.SemanticModel;
            var context = _selectionResult.GetContainingScope();
            var postProcessor = new PostProcessor(semanticModel, context.SpanStart);

            var statements = postProcessor.RemoveRedundantBlock(body.Statements.ToList());
            statements = postProcessor.RemoveDeclarationAssignmentPattern(statements);
            statements = postProcessor.RemoveInitializedDeclarationAndReturnPattern(statements);

            return SyntaxFactory.Block(statements);
        }

        private BlockSyntax SplitOrMoveDeclarationIntoMethodDefinition(BlockSyntax body, CancellationToken cancellationToken)
        {
            var statements = body.Statements;
            var postProcessor = new PostProcessor(_semanticDocument.SemanticModel, _selectionResult.FirstStatement().SpanStart);

            var variables = _analyzerResult.GetVariablesToSplitOrMoveIntoMethodDefinition(cancellationToken);
            var declarationStatements = CreateDeclarationStatements(variables);
            declarationStatements = postProcessor.MergeDeclarationStatements(declarationStatements);

            return SyntaxFactory.Block(declarationStatements.Concat(statements));
        }

        private IEnumerable<StatementSyntax> CreateDeclarationStatements(IEnumerable<VariableInfo> variables)
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

        private StatementSyntax CreateDeclarationStatement(VariableInfo variable, ExpressionSyntax initialValue)
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

        public InvocationExpressionSyntax CreateCallSignature()
        {
            var arguments = new List<ArgumentSyntax>();
            foreach (var argument in _analyzerResult.MethodParameters)
            {
                var modifier = GetParameterRefSyntaxKind(argument.ParameterModifier);
                var refOrOut = modifier == SyntaxKind.None ? default : SyntaxFactory.Token(modifier);

                arguments.Add(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(argument.Name)).WithRefOrOutKeyword(refOrOut));
            }

            var argumentListSyntax = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
            var methodName = SyntaxFactory.ParseExpression(_methodName);
            var invocation = SyntaxFactory.InvocationExpression(methodName, argumentListSyntax);
            return invocation;
        }

        private static SyntaxKind GetParameterRefSyntaxKind(ParameterBehavior parameterBehavior)
        {
            var parameterRefSyntaxKind = parameterBehavior == ParameterBehavior.Out ? SyntaxKind.OutKeyword : SyntaxKind.None;
            return parameterBehavior == ParameterBehavior.Ref ? SyntaxKind.RefKeyword : parameterRefSyntaxKind;
        }

        public async Task<SyntaxNode> GenerateBodyForCallSiteContainerAsync(CancellationToken cancellationToken)
        {
            var container = _selectionResult.GetContainingScope();
            var variableMapToRemove = CreateVariableDeclarationToRemoveMap(
                _analyzerResult.GetVariablesToMoveIntoMethodDefinition(cancellationToken), cancellationToken);
            var firstStatementToRemove = _selectionResult.FirstStatement();
            var lastStatementToRemove = _selectionResult.LastStatement();

            var statementsToInsert = await CreateStatementsOrInitializerToInsertAtCallSiteAsync(cancellationToken).ConfigureAwait(false);

            var callSiteGenerator =
                new CallSiteContainerRewriter(
                    container,
                    variableMapToRemove,
                    firstStatementToRemove,
                    lastStatementToRemove,
                    statementsToInsert);

            return callSiteGenerator.Generate();
        }

        private static HashSet<SyntaxAnnotation> CreateVariableDeclarationToRemoveMap(IEnumerable<VariableInfo> variables, CancellationToken cancellationToken)
        {
            var annotations = new List<Tuple<SyntaxToken, SyntaxAnnotation>>();

            foreach (var variable in variables)
            {
                variable.AddIdentifierTokenAnnotationPair(annotations, cancellationToken);
            }

            return new HashSet<SyntaxAnnotation>(annotations.Select(t => t.Item2));
        }

        private async Task<IEnumerable<SyntaxNode>> CreateStatementsOrInitializerToInsertAtCallSiteAsync(CancellationToken cancellationToken)
        {
            var semanticModel = _semanticDocument.SemanticModel;
            var context = _selectionResult.FirstStatement();
            var postProcessor = new PostProcessor(semanticModel, context.SpanStart);

            var statements = AddSplitOrMoveDeclarationOutStatementsToCallSite(cancellationToken);
            statements = postProcessor.MergeDeclarationStatements(statements);
            statements = AddAssignmentStatementToCallSite(statements);
            statements = AddInvocationAtCallSite(statements);

            return statements;
        }

        private IEnumerable<StatementSyntax> AddSplitOrMoveDeclarationOutStatementsToCallSite(CancellationToken cancellationToken)
        {
            var list = new List<StatementSyntax>();

            foreach (var variable in _analyzerResult.GetVariablesToSplitOrMoveOutToCallSite(cancellationToken))
            {
                if (variable.UseAsReturnValue)
                {
                    continue;
                }

                var declaration = CreateDeclarationStatement(variable, initialValue: null);
                list.Add(declaration);
            }

            return list;
        }

        private IEnumerable<StatementSyntax> AddAssignmentStatementToCallSite(IEnumerable<StatementSyntax> statements)
        {
            if (!_analyzerResult.HasVariableToUseAsReturnValue)
            {
                return statements;
            }

            var returnVariable = _analyzerResult.VariableToUseAsReturnValue;
            if (returnVariable.ReturnBehavior == ReturnBehavior.Initialization)
            {
                // there must be one decl behavior when there is "return value and initialize" variable
                var declarationStatement = CreateDeclarationStatement(returnVariable, CreateCallSignature());
                return statements.Concat(new[] { declarationStatement });
            }

            var identifier = SyntaxFactory.Identifier(returnVariable.Name);
            var invocation = CreateCallSignature();
            var assignment = CreateAssignmentExpressionStatement(identifier, invocation);
            return statements.Concat(new[] { assignment });
        }

        private static StatementSyntax CreateAssignmentExpressionStatement(SyntaxToken identifier, ExpressionSyntax rvalue)
            => SyntaxFactory.ExpressionStatement(CreateAssignmentExpression(identifier, rvalue));

        private static ExpressionSyntax CreateAssignmentExpression(SyntaxToken identifier, ExpressionSyntax rvalue)
        {
            return SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(identifier),
                rvalue);
        }

        private IEnumerable<StatementSyntax> AddInvocationAtCallSite(IEnumerable<StatementSyntax> statements)
        {
            if (_analyzerResult.HasVariableToUseAsReturnValue)
            {
                return statements;
            }

            // add invocation expression
            var statement = GetStatementContainingInvocationToExtractedMethodWorker();
            return statements.Concat(new[] { statement });
        }

        private StatementSyntax GetStatementContainingInvocationToExtractedMethodWorker()
        {
            var callSignature = CreateCallSignature();

            if (_analyzerResult.HasReturnType)
            {
                return SyntaxFactory.ReturnStatement(callSignature);
            }

            return SyntaxFactory.ExpressionStatement(callSignature);
        }
    }
}
