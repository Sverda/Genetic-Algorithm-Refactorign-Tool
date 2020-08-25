using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GenSharp.Refactorings.Analyzers.Helpers.Diagnostics
{
    internal static class DiagnosticVerifier
    {
        private static readonly MetadataReference _corlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference _systemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        private static readonly MetadataReference _cSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        private static readonly MetadataReference _codeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

        internal static string DefaultFilePathPrefix = "Test";
        internal static string CSharpDefaultFileExt = "cs";
        internal static string TestProjectName = "TestProject";

        public static Diagnostic[] GetSortedDiagnostics(string[] sources, DiagnosticAnalyzer analyzer)
        {
            return GetSortedDiagnosticsFromDocuments(analyzer, GetDocuments(sources));
        }

        public static Diagnostic[] GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer analyzer, Document[] documents)
        {
            var projects = new HashSet<Project>();
            foreach (var document in documents)
            {
                projects.Add(document.Project);
            }

            var diagnostics = new List<Diagnostic>();
            foreach (var project in projects)
            {
                var compilationWithAnalyzers = project.GetCompilationAsync().Result.WithAnalyzers(ImmutableArray.Create(analyzer));
                var diags = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
                foreach (var diag in diags)
                {
                    if (diag.Location == Location.None || diag.Location.IsInMetadata)
                    {
                        diagnostics.Add(diag);
                    }
                    else
                    {
                        for (var i = 0; i < documents.Length; i++)
                        {
                            var document = documents[i];
                            var tree = document.GetSyntaxTreeAsync().Result;
                            if (tree == diag.Location.SourceTree)
                            {
                                diagnostics.Add(diag);
                            }
                        }
                    }
                }
            }

            var results = SortDiagnostics(diagnostics);
            diagnostics.Clear();
            return results;
        }

        private static Diagnostic[] SortDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        }

        public static Document[] GetDocuments(string[] sources)
        {
            var project = CreateProject(sources);
            var documents = project.Documents.ToArray();

            if (sources.Length != documents.Length)
            {
                throw new InvalidOperationException("Amount of sources did not match amount of Documents created");
            }

            return documents;
        }

        private static Project CreateProject(string[] sources)
        {
            var fileNamePrefix = DefaultFilePathPrefix;

            var projectId = ProjectId.CreateNewId(debugName: TestProjectName);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, TestProjectName, TestProjectName, LanguageNames.CSharp)
                .AddMetadataReference(projectId, _corlibReference)
                .AddMetadataReference(projectId, _systemCoreReference)
                .AddMetadataReference(projectId, _cSharpSymbolsReference)
                .AddMetadataReference(projectId, _codeAnalysisReference);

            var count = 0;
            foreach (var source in sources)
            {
                var newFileName = fileNamePrefix + count + "." + CSharpDefaultFileExt;
                var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
                count++;
            }
            return solution.GetProject(projectId);
        }
    }
}
