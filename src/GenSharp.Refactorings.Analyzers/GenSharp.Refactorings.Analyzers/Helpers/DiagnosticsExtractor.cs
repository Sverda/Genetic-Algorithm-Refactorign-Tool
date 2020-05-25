using GenSharp.Refactorings.Analyzers.Helpers.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GenSharp.Refactorings.Analyzers.Helpers
{
    public class DiagnosticsExtractor
    {
        private readonly string _source;

        public DiagnosticsExtractor(string source)
        {
            _source = source;
        }

        public Diagnostic FromCode(int index)
        {
            var diagnostics = FromCode();
            return index >= diagnostics.Length ? null : diagnostics[index];
        }

        public Diagnostic[] FromCode()
        {
            var analyzer = GetCSharpDiagnosticAnalyzer();
            return DiagnosticVerifier.GetSortedDiagnostics(new[] { _source }, analyzer);
        }

        private static DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExtractStatementAnalyzer();
        }
    }
}
