using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using System;

namespace GenSharp.Genetics
{
    public class GeneticRunner
    {
        private ResultData _result;

        private int Generations { get; }
        private IFitness Fitness { get; }
        private IChromosome Chromosome { get; }
        private IPopulation Population { get; }
        private static ISelection Selection => new EliteSelection();
        private static ICrossover Crossover => new OnePointCrossover();
        private static IMutation Mutation => new ReverseSequenceMutation();

        public GeneticRunner(GeneticParameters parameters)
        {
            _ = parameters ?? throw new ArgumentNullException(nameof(parameters));

            Generations = parameters.Generations;
            Fitness = parameters.MetricsKind.MapFitness();
            Chromosome = new RefactoringChromosome(parameters.SequenceLength, parameters.Source);
            Population = new Population(parameters.MinPopulation, parameters.MaxPopulation, Chromosome);
        }

        public void Run()
        {
            var geneticAlgorithm = Configure();
            _result.InitialFitness = Chromosome.FullEvaluation();
            geneticAlgorithm.Start();
            _result.BestChromosomeSource = (geneticAlgorithm.BestChromosome as RefactoringChromosome)?.ApplyFixes();
            _result.FinalFitness = geneticAlgorithm.BestChromosome.FullEvaluation();
        }

        private GeneticAlgorithm Configure()
        {
            _result = new ResultData();
            var geneticAlgorithm = new GeneticAlgorithm(Population, Fitness, Selection, Crossover, Mutation)
            {
                Termination = new GenerationNumberTermination(Generations)
            };
            geneticAlgorithm.GenerationRan += GeneticAlgorithmOnGenerationRan;
            return geneticAlgorithm;
        }

        private void GeneticAlgorithmOnGenerationRan(object sender, EventArgs e)
        {
            if (!(sender is GeneticAlgorithm ga))
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if (ga.BestChromosome.Fitness != null)
            {
                _result.Fitness.Add(ga.BestChromosome.Fitness.Value);
            }
        }

        public ResultData CollectResult() => _result;
    }
}
