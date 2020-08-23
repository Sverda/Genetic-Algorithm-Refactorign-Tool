using GenSharp.Refactorings.Analyzers.Analyzers;
using GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

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
            var selectionResult = SelectionResult.ExtractFrom(extractFrom);
            var semanticDocument = await SemanticDocument.CreateAsync(document, cancellationToken);
            var analyzer = new MethodExtractorAnalyzer(semanticDocument, selectionResult, cancellationToken);

            var codeGenerator = new CodeGenerator(semanticDocument, analyzer.Analyze(), selectionResult, $"{extractFrom.Identifier.Text}_ExtractedMethod");
            semanticDocument = await codeGenerator.GenerateAsync(cancellationToken);

            return semanticDocument.Document;
        }
    }
}
