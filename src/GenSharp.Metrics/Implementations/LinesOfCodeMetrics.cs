using GenSharp.Metrics.Abstractions;
using GenSharp.Refactorings.Analyzers.Metrics;
using System.Threading;

namespace GenSharp.Metrics.Implementations
{
    public class LinesOfCodeMetrics : IEvaluateMetric
    {
        private readonly string _source;

        public LinesOfCodeMetrics(string source)
        {
            _source = source;
        }

        public double Evaluate()
        {
            return MetricsFacade.LinesOfCode(_source, CancellationToken.None).Result;
        }
    }
}
