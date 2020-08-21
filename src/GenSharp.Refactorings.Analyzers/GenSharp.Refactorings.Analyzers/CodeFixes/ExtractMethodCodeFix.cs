using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenSharp.Refactorings.Analyzers.Analyzers;
using GenSharp.Refactorings.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace GenSharp.Refactorings.Analyzers.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExtractMethodCodeFix)), Shared]
    public class ExtractMethodCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ExtractMethodAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
        {
            return null;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            var codeAction = CodeAction.Create(DiagnosticDescriptors.ExtractMethod.Title.ToString(), cancellationToken => ExtractMethodAsync(context.Document, declaration, cancellationToken));
            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static async Task<Document> ExtractMethodAsync(Document document, MethodDeclarationSyntax extractFrom, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

            var statements = FindExtractableCode(extractFrom);
            var extractedMethod = ConstructMethodDeclaration(extractFrom, statements);
            InsertMethod(extractFrom, editor, extractedMethod);
            ReplaceStatementsWithOneNode(extractedMethod, editor, statements);

            return editor.GetChangedDocument();
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

        private static MethodDeclarationSyntax ConstructMethodDeclaration(MethodDeclarationSyntax extractFrom, IEnumerable<StatementSyntax> statements)
        {
            var extractedMethod = SyntaxFactory
                .MethodDeclaration(SyntaxFactory.ParseTypeName("void"), $"{extractFrom.Identifier.Text}_ExtractedMethod")
                .WithBody(SyntaxFactory.Block(statements))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
            return extractedMethod;
        }

        private static void InsertMethod(SyntaxNode extractFrom, SyntaxEditor editor, SyntaxNode extractedMethod)
        {
            var classNode = extractFrom.Ancestors(false).OfType<ClassDeclarationSyntax>().Single();
            editor.AddMember(classNode, extractedMethod);
        }

        private static void ReplaceStatementsWithOneNode(MethodDeclarationSyntax extractedMethod, SyntaxEditor editor, IEnumerable<StatementSyntax> statements)
        {
            var methodCall = ConstructMethodCall(extractedMethod);
            editor.ReplaceNode(statements.First(), methodCall);
            foreach (var statementSyntax in statements.Skip(1))
            {
                editor.RemoveNode(statementSyntax);
            }
        }

        private static ExpressionStatementSyntax ConstructMethodCall(MethodDeclarationSyntax method)
        {
            var methodAccess = SyntaxFactory.ParseExpression(method.Identifier.ValueText);
            var methodCall = SyntaxFactory.ExpressionStatement
            (
                SyntaxFactory.InvocationExpression(methodAccess, SyntaxFactory.ArgumentList())
            );
            return methodCall
                .NormalizeWhitespace()
                .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
        }
    }
}
