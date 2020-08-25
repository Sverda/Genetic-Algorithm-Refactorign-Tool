using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using GenSharp.Metrics.Abstractions;
using GenSharp.Refactorings.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace GenSharp.Genetics
{
    public class MetricsFitness<TMetrics> : IFitness where TMetrics : IEvaluateMetric, new()
    {
        public double Evaluate(IChromosome chromosome)
        {
            var sequence = chromosome.GetGenes().Select(g => g.Value).Cast<Diagnostic>();
            var refactoringChromosome = chromosome as RefactoringChromosome;
            var newSource = CodeFixApplier.ComputeCodeFixes(refactoringChromosome.Source, sequence);
            return new TMetrics()
                .SetSource(newSource)
                .Evaluate();
        }
    }
}
