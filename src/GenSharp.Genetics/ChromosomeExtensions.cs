using GeneticSharp.Domain.Chromosomes;
using GenSharp.Metrics.Implementations;

namespace GenSharp.Genetics
{
    public static class ChromosomeExtensions
    {
        public static MetricsResults FullEvaluation(this IChromosome chromosome)
        {
            return new MetricsResults()
            {
                CyclomaticComplexity = new MetricsFitness<CyclomaticComplexityMetrics>().Evaluate(chromosome),
                LinesOfCode = new MetricsFitness<LinesOfCodeMetrics>().Evaluate(chromosome),
                MaintainabilityIndex = new MetricsFitness<MaintainabilityIndexMetrics>().Evaluate(chromosome)
            };
        }
    }
}
