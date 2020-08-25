using GenSharp.Metrics.Abstractions;
using GenSharp.Refactorings.Analyzers.Metrics;
using System.Threading;

namespace GenSharp.Metrics.Implementations
{
    public class MaintainabilityIndexMetrics : IEvaluateMetric
    {
        private readonly string _source;

        public MaintainabilityIndexMetrics(string source)
        {
            _source = source;
        }

        public double Evaluate()
        {
            return MetricsFacade.MaintainabilityIndex(_source, CancellationToken.None).Result;
        }
    }
}
