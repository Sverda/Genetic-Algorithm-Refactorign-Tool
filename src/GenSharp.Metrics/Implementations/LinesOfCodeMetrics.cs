using GenSharp.Metrics.Abstractions;
using GenSharp.Refactorings.Analyzers.Metrics;
using System.Threading;

namespace GenSharp.Metrics.Implementations
{
    public class LinesOfCodeMetrics : IEvaluateMetric
    {
        private string _source;

        public IEvaluateMetric SetSource(string source)
        {
            _source = source;

            return this;
        }

        public double Evaluate()
        {
            return MetricsFacade.LinesOfCode(_source, CancellationToken.None).Result;
        }
    }
}
