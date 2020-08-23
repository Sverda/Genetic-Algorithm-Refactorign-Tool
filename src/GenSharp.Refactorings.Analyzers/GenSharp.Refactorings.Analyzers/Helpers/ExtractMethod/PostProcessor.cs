using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal class PostProcessor
    {
        private readonly SemanticModel _semanticModel;
        private readonly int _contextPosition;

        public PostProcessor(SemanticModel semanticModel, int contextPosition = 0)
        {
            _semanticModel = semanticModel;
            _contextPosition = contextPosition;
        }

        public IEnumerable<StatementSyntax> MergeDeclarationStatements(IEnumerable<StatementSyntax> statements)
        {
            if (statements.FirstOrDefault() == null)
            {
                return statements;
            }

            return MergeDeclarationStatementsWorker(statements);
        }

        private IEnumerable<StatementSyntax> MergeDeclarationStatementsWorker(IEnumerable<StatementSyntax> statements)
        {
            var map = new Dictionary<ITypeSymbol, List<LocalDeclarationStatementSyntax>>();
            foreach (var statement in statements)
            {
                if (!IsDeclarationMergable(statement))
                {
                    foreach (var declStatement in GetMergedDeclarationStatements(map))
                    {
                        yield return declStatement;
                    }

                    yield return statement;
                    continue;
                }

                AppendDeclarationStatementToMap(statement as LocalDeclarationStatementSyntax, map);
            }

            // merge leftover
            if (map.Count <= 0)
            {
                yield break;
            }

            foreach (var declStatement in GetMergedDeclarationStatements(map))
            {
                yield return declStatement;
            }
        }

        private IEnumerable<LocalDeclarationStatementSyntax> GetMergedDeclarationStatements(
            Dictionary<ITypeSymbol, List<LocalDeclarationStatementSyntax>> map)
        {
            foreach (var keyValuePair in map)
            {
                // merge all variable decl for current type
                var variables = new List<VariableDeclaratorSyntax>();
                foreach (var statement in keyValuePair.Value)
                {
                    foreach (var variable in statement.Declaration.Variables)
                    {
                        variables.Add(variable);
                    }
                }

                // and create one decl statement
                // use type name from the first decl statement
                yield return
                    SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(keyValuePair.Value.First().Declaration.Type, SyntaxFactory.SeparatedList(variables)));
            }

            map.Clear();
        }

        private void AppendDeclarationStatementToMap(
            LocalDeclarationStatementSyntax statement,
            Dictionary<ITypeSymbol, List<LocalDeclarationStatementSyntax>> map)
        {
            var type = ModelExtensions.GetSpeculativeTypeInfo(_semanticModel, _contextPosition, statement.Declaration.Type, SpeculativeBindingOption.BindAsTypeOrNamespace).Type;

            map.GetOrAdd(type, _ => new List<LocalDeclarationStatementSyntax>()).Add(statement);
        }

        private bool IsDeclarationMergable(StatementSyntax statement)
        {
            // to be mergable, statement must be
            // 1. decl statement without any extra info
            // 2. no initialization on any of its decls
            // 3. no trivia except whitespace
            // 4. type must be known

            if (!(statement is LocalDeclarationStatementSyntax declarationStatement))
            {
                return false;
            }

            if (declarationStatement.Modifiers.Count > 0 ||
                declarationStatement.IsConst ||
                declarationStatement.IsMissing)
            {
                return false;
            }

            if (ContainsAnyInitialization(declarationStatement))
            {
                return false;
            }

            if (!ContainsOnlyWhitespaceTrivia(declarationStatement))
            {
                return false;
            }

            var semanticInfo = ModelExtensions.GetSpeculativeTypeInfo(_semanticModel, _contextPosition, declarationStatement.Declaration.Type, SpeculativeBindingOption.BindAsTypeOrNamespace).Type;
            if (semanticInfo == null ||
                semanticInfo.TypeKind == TypeKind.Error ||
                semanticInfo.TypeKind == TypeKind.Unknown)
            {
                return false;
            }

            return true;
        }

        private bool ContainsAnyInitialization(LocalDeclarationStatementSyntax statement)
        {
            foreach (var variable in statement.Declaration.Variables)
            {
                if (variable.Initializer != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsOnlyWhitespaceTrivia(StatementSyntax statement)
        {
            foreach (var token in statement.DescendantTokens())
            {
                foreach (var trivia in token.LeadingTrivia.Concat(token.TrailingTrivia))
                {
                    if (trivia.Kind() != SyntaxKind.WhitespaceTrivia &&
                        trivia.Kind() != SyntaxKind.EndOfLineTrivia)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
