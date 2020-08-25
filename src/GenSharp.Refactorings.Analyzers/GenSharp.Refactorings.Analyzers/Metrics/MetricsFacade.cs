using GenSharp.Refactorings.Analyzers.Helpers.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GenSharp.Refactorings.Analyzers.Metrics
{
    public static class MetricsFacade
    {
        public static async Task<double> CyclomaticComplexity(string source, CancellationToken cancellationToken)
        {
            var metrics = await CodeAnalysisMetricData(source, cancellationToken);
            return metrics.CyclomaticComplexity;
        }
        
        public static async Task<double> MaintainabilityIndex(string source, CancellationToken cancellationToken)
        {
            var metrics = await CodeAnalysisMetricData(source, cancellationToken);
            return metrics.MaintainabilityIndex;
        }
        
        public static async Task<double> LinesOfCode(string source, CancellationToken cancellationToken)
        {
            var metrics = await CodeAnalysisMetricData(source, cancellationToken);
            return metrics.ExecutableLines;
        }

        private static async Task<CodeAnalysisMetricData> CodeAnalysisMetricData(string source, CancellationToken cancellationToken)
        {
            var document = DiagnosticVerifier.GetDocuments(new[] {source}).First();
            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);
            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString(), new[] {syntaxTree});
            var metrics = await Metrics.CodeAnalysisMetricData.ComputeAsync(compilation, cancellationToken);
            return metrics;
        }
    }
}
