using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GenSharp.Metrics.Implementations;
using System;

namespace GenSharp.Genetics
{
    public class GeneticRunner
    {
        private readonly GeneticParameters _parameters;

        public GeneticRunner(GeneticParameters parameters)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public void Run(string source)
        {
            var ga = ConfigureGeneticAlgorithm(source);
            ga.Start();
        }

        private GeneticAlgorithm ConfigureGeneticAlgorithm(string source)
        {
            var selection = new EliteSelection();
            var crossover = new OrderedCrossover();
            var mutation = new ReverseSequenceMutation();
            var fitness = ResolveMetricsKind();
            var chromosome = new RefactoringChromosome(_parameters.SequenceLength, source);
            var population = new Population(_parameters.MinPopulation, _parameters.MaxPopulation, chromosome);

            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                Termination = new GenerationNumberTermination(_parameters.Generations)
            };
            return ga;
        }

        private IFitness ResolveMetricsKind()
        {
            switch (_parameters.MetricsKind)
            {
                case MetricsKind.CyclomaticComplexity:
                    return new MetricsFitness<CyclomaticComplexityMetrics>();

                case MetricsKind.LinesOfCode:
                    return new MetricsFitness<LinesOfCodeMetrics>();

                case MetricsKind.MaintainabilityIndex:
                    return new MetricsFitness<MaintainabilityIndexMetrics>();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
