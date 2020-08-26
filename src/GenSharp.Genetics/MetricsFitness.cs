using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using GenSharp.Metrics.Abstractions;

namespace GenSharp.Genetics
{
    public class MetricsFitness<TMetrics> : IFitness where TMetrics : IEvaluateMetric, new()
    {
        public double Evaluate(IChromosome chromosome)
        {
            var refactoringChromosome = chromosome as RefactoringChromosome;
            var newSource = refactoringChromosome.ApplyFixes();
            return new TMetrics()
                .SetSource(newSource)
                .Evaluate();
        }
    }
}
