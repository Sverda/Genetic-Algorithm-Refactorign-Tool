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

        private async Task<Document> ExtractMethodAsync(Document document, MethodDeclarationSyntax declaration, CancellationToken cancellationToken)
        {

            return document;
        }
    }
}
