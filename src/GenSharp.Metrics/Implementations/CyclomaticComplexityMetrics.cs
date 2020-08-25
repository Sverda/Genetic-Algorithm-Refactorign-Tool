using GenSharp.Metrics.Abstractions;
using GenSharp.Refactorings.Analyzers.Metrics;
using System.Threading;

namespace GenSharp.Metrics.Implementations
{
    public class CyclomaticComplexityMetrics : IEvaluateMetric
    {
        private readonly string _source;

        public CyclomaticComplexityMetrics(string source)
        {
            _source = source;
        }

        public double Evaluate()
        {
            return MetricsFacade.CyclomaticComplexity(_source, CancellationToken.None).Result;
        }
    }
}
