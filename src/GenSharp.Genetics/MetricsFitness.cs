using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using GenSharp.Metrics.Implementations;
using GenSharp.Refactorings.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace GenSharp.Genetics
{
    public class MetricsFitness : IFitness
    {
        public double Evaluate(IChromosome chromosome)
        {
            var sequence = chromosome.GetGenes().Select(g => g.Value).Cast<Diagnostic>();
            var refactoringChromosome = chromosome as RefactoringChromosome;
            var newSource = CodeFixApplier.ComputeCodeFixes(refactoringChromosome.Source, sequence);
            return new CyclomaticComplexityMetrics(newSource).Evaluate();
        }
    }
}
