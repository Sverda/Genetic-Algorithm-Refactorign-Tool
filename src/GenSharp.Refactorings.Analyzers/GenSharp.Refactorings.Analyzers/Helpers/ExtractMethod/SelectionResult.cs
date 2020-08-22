using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal class SelectionResult
    {
        public static SelectionResult ExtractFrom(BaseMethodDeclarationSyntax method)
        {
            var statements = FindExtractableCode(method);
            return new SelectionResult(statements);
        }

        private static IEnumerable<StatementSyntax> FindExtractableCode(BaseMethodDeclarationSyntax extractFrom)
        {
            var random = new Random();
            var returnNodes = new List<StatementSyntax>();
            var statementsCount = extractFrom.Body.Statements.Count;
            var startingPosition = random.Next(0, statementsCount);
            var depth = random.Next(1,( statementsCount + 1) - startingPosition);
            for (var i = startingPosition; i < startingPosition + depth; i++)
            {
                var node = extractFrom.Body.Statements[i];
                if (node != null) 
                {
                    returnNodes.Add(node);
                }
            }

            return returnNodes;
        }

        private readonly IEnumerable<StatementSyntax> _statements;

        public SelectionResult(IEnumerable<StatementSyntax> statements)
        {
            _statements = statements ?? throw new ArgumentNullException(nameof(statements));
        }

        public BlockSyntax AsBody()
        {
            return SyntaxFactory.Block(_statements);
        }

        public SyntaxNode FirstStatement()
        {
            return _statements.First();
        }

        public IEnumerable<SyntaxNode> OtherThanFirstStatements()
        {
            return _statements.Skip(1);
        }

        public SyntaxNode LastStatement()
        {
            return _statements.Last();
        }

        public SyntaxNode GetContainingScope()
        {
            return _statements.First().Ancestors(false).FirstOrDefault(n => n is AccessorDeclarationSyntax ||
                                                                            n is LocalFunctionStatementSyntax ||
                                                                            n is BaseMethodDeclarationSyntax ||
                                                                            n is ParenthesizedLambdaExpressionSyntax ||
                                                                            n is SimpleLambdaExpressionSyntax ||
                                                                            n is AnonymousMethodExpressionSyntax ||
                                                                            n is CompilationUnitSyntax);
        }

        public TextSpan GetFinalSpan()
        {
            var first = _statements.First();
            var last = _statements.Last();
            var firstStart = first.GetLocation().SourceSpan.Start;
            var firstLength = first.GetLocation().SourceSpan.Length;
            if (first == last)
            {
                return new TextSpan(firstStart, firstLength);
            }

            var lastLength = last.GetLocation().SourceSpan.Length;
            return new TextSpan(firstStart, firstLength + lastLength);
        }

        public ITypeSymbol GetContainingScopeType(SemanticModel semanticModel)
        {
            var node = GetContainingScope();

            switch (node)
            {
                case AccessorDeclarationSyntax access:
                    // property or event case
                    if (access.Parent == null || access.Parent.Parent == null)
                    {
                        return null;
                    }

                    switch (semanticModel.GetDeclaredSymbol(access.Parent.Parent))
                    {
                        case IPropertySymbol propertySymbol:
                            return propertySymbol.Type;
                        case IEventSymbol eventSymbol:
                            return eventSymbol.Type;
                        default:
                            return null;
                    }

                case MethodDeclarationSyntax method:
                    return semanticModel.GetDeclaredSymbol(method).ReturnType;

                case ParenthesizedLambdaExpressionSyntax lambda:
                    return semanticModel.GetLambdaOrAnonymousMethodReturnType(lambda);

                case SimpleLambdaExpressionSyntax lambda:
                    return semanticModel.GetLambdaOrAnonymousMethodReturnType(lambda);

                case AnonymousMethodExpressionSyntax anonymous:
                    return semanticModel.GetLambdaOrAnonymousMethodReturnType(anonymous);

                default:
                    return null;
            }
        }
    }
}
