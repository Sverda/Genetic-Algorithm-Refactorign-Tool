using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal class CallSiteContainerRewriter : CSharpSyntaxRewriter
    {
        private readonly SyntaxNode _outmostCallSiteContainer;
        private readonly IEnumerable<SyntaxNode> _statementsOrMemberOrAccessorToInsert;
        private readonly HashSet<SyntaxAnnotation> _variableToRemoveMap;
        private readonly SyntaxNode _firstStatementOrFieldToReplace;
        private readonly SyntaxNode _lastStatementOrFieldToReplace;

        public CallSiteContainerRewriter(
            SyntaxNode outmostCallSiteContainer,
            HashSet<SyntaxAnnotation> variableToRemoveMap,
            SyntaxNode firstStatementOrFieldToReplace,
            SyntaxNode lastStatementOrFieldToReplace,
            IEnumerable<SyntaxNode> statementsOrFieldToInsert)
        {
            _outmostCallSiteContainer = outmostCallSiteContainer;

            _variableToRemoveMap = variableToRemoveMap;
            _statementsOrMemberOrAccessorToInsert = statementsOrFieldToInsert;

            _firstStatementOrFieldToReplace = firstStatementOrFieldToReplace;
            _lastStatementOrFieldToReplace = lastStatementOrFieldToReplace;
        }

        public SyntaxNode Generate()
            => Visit(_outmostCallSiteContainer);

        private SyntaxNode ContainerOfStatementsOrFieldToReplace => _firstStatementOrFieldToReplace.Parent;

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            node = (LocalDeclarationStatementSyntax)base.VisitLocalDeclarationStatement(node);
            var list = new List<VariableDeclaratorSyntax>();
            var triviaList = new List<SyntaxTrivia>();
            // go through each var decls in decl statement
            foreach (var variable in node.Declaration.Variables)
            {
                if (_variableToRemoveMap.HasSyntaxAnnotation(variable))
                {
                    // we don't remove trivia around tokens we remove
                    triviaList.AddRange(variable.GetLeadingTrivia());
                    triviaList.AddRange(variable.GetTrailingTrivia());
                    continue;
                }

                if (triviaList.Count > 0)
                {
                    list.Add(variable.WithPrependedLeadingTrivia(triviaList));
                    triviaList.Clear();
                    continue;
                }

                list.Add(variable);
            }

            if (list.Count == 0)
            {
                // nothing has survived. remove this from the list
                if (triviaList.Count == 0)
                {
                    return null;
                }

                // well, there are trivia associated with the node.
                // we can't just delete the node since then, we will lose
                // the trivia. unfortunately, it is not easy to attach the trivia
                // to next token. for now, create an empty statement and associate the
                // trivia to the statement

                // TODO : think about a way to move the trivia to next token.
                return SyntaxFactory.EmptyStatement(SyntaxFactory.Token(SyntaxFactory.TriviaList(triviaList), SyntaxKind.SemicolonToken, SyntaxTriviaList.Create(SyntaxFactory.ElasticMarker)));
            }

            if (list.Count == node.Declaration.Variables.Count)
            {
                // nothing has changed, return as it is
                return node;
            }

            // TODO : fix how it manipulate trivia later

            // if there is left over syntax trivia, it will be attached to leading trivia
            // of semicolon
            return
                SyntaxFactory.LocalDeclarationStatement(
                    node.Modifiers,
                        SyntaxFactory.VariableDeclaration(
                            node.Declaration.Type,
                            SyntaxFactory.SeparatedList(list)),
                            node.SemicolonToken.WithPrependedLeadingTrivia(triviaList));
        }

        // for every kind of extract methods
        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                // make sure we visit nodes under the block
                return base.VisitBlock(node);
            }
            
            return node.WithStatements(VisitList(ReplaceStatements(node.Statements)).ToSyntaxList());
        }

        public override SyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                // make sure we visit nodes under the switch section
                return base.VisitSwitchSection(node);
            }

            return node.WithStatements(VisitList(ReplaceStatements(node.Statements)).ToSyntaxList());
        }

        // only for single statement or expression
        public override SyntaxNode VisitLabeledStatement(LabeledStatementSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitLabeledStatement(node);
            }

            return node.WithStatement(ReplaceStatementIfNeeded(node.Statement));
        }

        public override SyntaxNode VisitElseClause(ElseClauseSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitElseClause(node);
            }

            return node.WithStatement(ReplaceStatementIfNeeded(node.Statement));
        }

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitIfStatement(node);
            }

            return node.WithCondition(VisitNode(node.Condition))
                       .WithStatement(ReplaceStatementIfNeeded(node.Statement))
                       .WithElse(VisitNode(node.Else));
        }

        public override SyntaxNode VisitLockStatement(LockStatementSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitLockStatement(node);
            }

            return node.WithExpression(VisitNode(node.Expression))
                       .WithStatement(ReplaceStatementIfNeeded(node.Statement));
        }

        public override SyntaxNode VisitFixedStatement(FixedStatementSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitFixedStatement(node);
            }

            return node.WithDeclaration(VisitNode(node.Declaration))
                       .WithStatement(ReplaceStatementIfNeeded(node.Statement));
        }

        public override SyntaxNode VisitUsingStatement(UsingStatementSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitUsingStatement(node);
            }

            return node.WithDeclaration(VisitNode(node.Declaration))
                       .WithExpression(VisitNode(node.Expression))
                       .WithStatement(ReplaceStatementIfNeeded(node.Statement));
        }

        public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitForEachStatement(node);
            }

            return node.WithExpression(VisitNode(node.Expression))
                       .WithStatement(ReplaceStatementIfNeeded(node.Statement));
        }

        public override SyntaxNode VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitForEachVariableStatement(node);
            }

            return node.WithExpression(VisitNode(node.Expression))
                       .WithStatement(ReplaceStatementIfNeeded(node.Statement));
        }

        public override SyntaxNode VisitForStatement(ForStatementSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitForStatement(node);
            }

            return node.WithDeclaration(VisitNode(node.Declaration))
                       .WithInitializers(VisitList(node.Initializers))
                       .WithCondition(VisitNode(node.Condition))
                       .WithIncrementors(VisitList(node.Incrementors))
                       .WithStatement(ReplaceStatementIfNeeded(node.Statement));
        }

        public override SyntaxNode VisitDoStatement(DoStatementSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitDoStatement(node);
            }

            return node.WithStatement(ReplaceStatementIfNeeded(node.Statement))
                       .WithCondition(VisitNode(node.Condition));
        }

        public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitWhileStatement(node);
            }

            return node.WithCondition(VisitNode(node.Condition))
                       .WithStatement(ReplaceStatementIfNeeded(node.Statement));
        }

        private TNode VisitNode<TNode>(TNode node) where TNode : SyntaxNode
            => (TNode)Visit(node);

        private StatementSyntax ReplaceStatementIfNeeded(StatementSyntax statement)
        {
            // if all three same
            if ((statement != _firstStatementOrFieldToReplace) || (_firstStatementOrFieldToReplace != _lastStatementOrFieldToReplace))
            {
                return statement;
            }

            // replace one statement with another
            if (_statementsOrMemberOrAccessorToInsert.Count() == 1)
            {
                return _statementsOrMemberOrAccessorToInsert.Cast<StatementSyntax>().Single();
            }

            // replace one statement with multiple statements (see bug # 6310)
            return SyntaxFactory.Block(SyntaxFactory.List(_statementsOrMemberOrAccessorToInsert.Cast<StatementSyntax>()));
        }

        private SyntaxList<TSyntax> ReplaceList<TSyntax>(SyntaxList<TSyntax> list)
            where TSyntax : SyntaxNode
        {
            // okay, this visit contains the statement
            var newList = new List<TSyntax>(list);

            var firstIndex = newList.FindIndex(s => s == _firstStatementOrFieldToReplace);

            var lastIndex = newList.FindIndex(s => s == _lastStatementOrFieldToReplace);

            // remove statement that must be removed
            newList.RemoveRange(firstIndex, lastIndex - firstIndex + 1);

            // add new statements to replace
            newList.InsertRange(firstIndex, _statementsOrMemberOrAccessorToInsert.Cast<TSyntax>());

            return newList.ToSyntaxList();
        }

        private SyntaxList<StatementSyntax> ReplaceStatements(SyntaxList<StatementSyntax> statements)
            => ReplaceList(statements);

        private SyntaxList<AccessorDeclarationSyntax> ReplaceAccessors(SyntaxList<AccessorDeclarationSyntax> accessors)
            => ReplaceList(accessors);

        private SyntaxList<MemberDeclarationSyntax> ReplaceMembers(SyntaxList<MemberDeclarationSyntax> members, bool global)
        {
            // okay, this visit contains the statement
            var newMembers = new List<MemberDeclarationSyntax>(members);

            var firstMemberIndex = newMembers.FindIndex(s => s == (global ? _firstStatementOrFieldToReplace.Parent : _firstStatementOrFieldToReplace));

            var lastMemberIndex = newMembers.FindIndex(s => s == (global ? _lastStatementOrFieldToReplace.Parent : _lastStatementOrFieldToReplace));

            // remove statement that must be removed
            newMembers.RemoveRange(firstMemberIndex, lastMemberIndex - firstMemberIndex + 1);

            // add new statements to replace
            newMembers.InsertRange(firstMemberIndex,
                _statementsOrMemberOrAccessorToInsert.Select(s => global ? SyntaxFactory.GlobalStatement((StatementSyntax)s) : (MemberDeclarationSyntax)s));

            return newMembers.ToSyntaxList();
        }

        public override SyntaxNode VisitGlobalStatement(GlobalStatementSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitGlobalStatement(node);
            }

            return node.WithStatement(ReplaceStatementIfNeeded(node.Statement));
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitConstructorDeclaration(node);
            }

            return node.WithInitializer((ConstructorInitializerSyntax)_statementsOrMemberOrAccessorToInsert.Single());
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitClassDeclaration(node);
            }

            var newMembers = VisitList(ReplaceMembers(node.Members, global: false));
            return node.WithMembers(newMembers);
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitStructDeclaration(node);
            }

            var newMembers = VisitList(ReplaceMembers(node.Members, global: false));
            return node.WithMembers(newMembers);
        }

        public override SyntaxNode VisitAccessorList(AccessorListSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace)
            {
                return base.VisitAccessorList(node);
            }

            var newAccessors = VisitList(ReplaceAccessors(node.Accessors));
            return node.WithAccessors(newAccessors);
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            if (node != ContainerOfStatementsOrFieldToReplace.Parent)
            {
                // make sure we visit nodes under the block
                return base.VisitCompilationUnit(node);
            }

            var newMembers = VisitList(ReplaceMembers(node.Members, global: true));
            return node.WithMembers(newMembers);
        }
    }
}
