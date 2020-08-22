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

            var selectionResult = SelectionResult.ExtractFrom(extractFrom);

            var semanticDocument = await SemanticDocument.CreateAsync(document, cancellationToken);
            var analyzer = new MethodExtractorAnalyzer(semanticDocument, selectionResult, cancellationToken);
            var result = analyzer.Analyze();

            var extractedMethod = CodeGenerator.ConstructMethodDeclaration(extractFrom, selectionResult, result);
            InsertMethod(extractFrom, editor, extractedMethod);
            ReplaceStatementsWithOneNode(editor, extractedMethod, selectionResult, result);

            return editor.GetChangedDocument();
        }

        private static void InsertMethod(SyntaxNode extractFrom, SyntaxEditor editor, SyntaxNode extractedMethod)
        {
            var classNode = extractFrom.Ancestors(false).OfType<ClassDeclarationSyntax>().Single();
            editor.AddMember(classNode, extractedMethod);
        }

        private static void ReplaceStatementsWithOneNode(SyntaxEditor editor, MethodDeclarationSyntax extractedMethod, SelectionResult selectionResult, AnalyzerResult analyzerResult)
        {
            var methodCall = CodeGenerator.CreateCallSignature(extractedMethod, analyzerResult);
            editor.ReplaceRangeNodes(selectionResult, methodCall);
        }
    }
}
