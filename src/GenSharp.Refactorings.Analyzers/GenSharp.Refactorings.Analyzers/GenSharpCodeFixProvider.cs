using GenSharp.Refactorings.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GenSharp.Refactorings.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GenSharpCodeFixProvider)), Shared]
    public class GenSharpCodeFixProvider : CodeFixProvider
    {
        private ClassDeclarationSyntax _currentClassNode;
        private GenerateMethodFromStatementSyntaxWalker _methodGenerator;
        private const string _title = "Extract declaration";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ExtractStatementAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
        {
            return null;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclarationSyntax>().First();

            var codeAction = CodeAction.Create(_title, c => ExtractVariableDeclarationAsync(context.Document, declaration, c));
            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private async Task<Document> ExtractVariableDeclarationAsync(Document document, VariableDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var oldClassNode = declaration.Ancestors(false).OfType<ClassDeclarationSyntax>().Single();
            _currentClassNode = oldClassNode;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            _methodGenerator = new GenerateMethodFromStatementSyntaxWalker(semanticModel);
            _methodGenerator.VisitVariableDeclaration(declaration);

            RemoveVariableDeclarations();
            InsertExtractedMethods();
            ReplaceVariableCallsWithMethodCalls();

            var rootWithNewClass = document.GetSyntaxTreeAsync().Result.GetRoot().ReplaceNode(oldClassNode, _currentClassNode);
            return document.WithSyntaxRoot(rootWithNewClass);
        }

        private void RemoveVariableDeclarations()
        {
            var parentTargetNodes = _methodGenerator.ExtractedStatements.Select(m => m.TargetStatement.Parent);
            _currentClassNode = _currentClassNode.TrackNodes(parentTargetNodes);
            RemoveLeadingLine(_currentClassNode.GetCurrentNodes(parentTargetNodes));
            _currentClassNode = _currentClassNode.RemoveNodes(_currentClassNode.GetCurrentNodes(parentTargetNodes), SyntaxRemoveOptions.KeepNoTrivia);
        }

        private void RemoveLeadingLine(IEnumerable<SyntaxNode> parentTargetNodes)
        {
            foreach (var parentTargetNode in parentTargetNodes)
            {
                var nextSibling = parentTargetNode.GetNextNode();
                var emptyLineTrivia = nextSibling.DescendantTrivia().First();
                if (!emptyLineTrivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    continue;
                }

                var siblingWithoutLeadingLine = nextSibling.ReplaceTrivia(emptyLineTrivia, new SyntaxTrivia());
                _currentClassNode = _currentClassNode.ReplaceNode(nextSibling, siblingWithoutLeadingLine);
            }
        }

        private void InsertExtractedMethods()
        {
            var members = _methodGenerator.ExtractedStatements.Select(model => model.Method).ToArray();
            _currentClassNode = _currentClassNode.AddMembers(members);
        }

        private void ReplaceVariableCallsWithMethodCalls()
        {
            foreach (var model in _methodGenerator.ExtractedStatements)
            {
                var variableName = model.TargetStatement.DescendantNodes().OfType<VariableDeclaratorSyntax>().Select(s => s.Identifier).Single();
                var variableUses = _currentClassNode.DescendantNodes().OfType<IdentifierNameSyntax>().Where(i => i.Identifier.Value.Equals(variableName.Value));
                _currentClassNode = _currentClassNode.ReplaceNodes(variableUses, (original, replaced) => model.Call.Expression);
            }
        }
    }
}
