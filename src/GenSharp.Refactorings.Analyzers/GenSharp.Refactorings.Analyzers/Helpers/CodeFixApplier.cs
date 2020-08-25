using GenSharp.Refactorings.Analyzers.CodeFixes;
using GenSharp.Refactorings.Analyzers.Helpers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GenSharp.Refactorings.Analyzers.Helpers
{
    public static class CodeFixApplier
    {
        public static string ComputeCodeFixes(string source, IEnumerable<Diagnostic> sequence)
        {
            var newSource = source;
            foreach (var diagnostic in sequence)
            {
                var codeFix = ResolveCodeFix(diagnostic.Id);
                newSource = ApplyFix(codeFix, diagnostic, source);
            }

            return newSource;
        }

        private static CodeFixProvider ResolveCodeFix(string diagnosticId)
        {
            switch (diagnosticId)
            {
                case DiagnosticIdentifiers.ExtractStatement:
                    return new ExtractStatementCodeFix();
                case DiagnosticIdentifiers.ExtractMethod:
                    return new ExtractMethodCodeFix();
                default:
                    throw new ArgumentOutOfRangeException(nameof(diagnosticId));
            }
        }

        private static string ApplyFix(CodeFixProvider codeFixProvider, Diagnostic diagnostic, string oldSource)
        {
            var document = DiagnosticVerifier.GetDocuments(new[] { oldSource }).Single();

            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
            codeFixProvider.RegisterCodeFixesAsync(context).Wait();

            if (!actions.Any())
            {
                return oldSource;
            }

            document = ApplyFix(document, actions.ElementAt(0));

            var actual = GetStringFromDocument(document);
            return actual;
        }

        private static Document ApplyFix(TextDocument document, CodeAction codeAction)
        {
            var operations = codeAction.GetOperationsAsync(CancellationToken.None).Result;
            var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            return solution.GetDocument(document.Id);
        }

        private static string GetStringFromDocument(Document document)
        {
            document = Formatter.FormatAsync(document).Result;
            var root = document.GetSyntaxRootAsync().Result;
            return root.ToFullString();
        }
    }
}
