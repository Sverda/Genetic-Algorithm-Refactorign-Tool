using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GenSharp.Metrics.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenSharp.Genetics.Test
{
    [TestClass]
    public class GaTests
    {
        [TestMethod]
        public void CyclomaticComplexity()
        {
            const string source = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class Calculations
    {
        double CalculateTotal(int quantity, int itemPrice)
        {
            double basePrice = quantity * itemPrice;

            if (basePrice > 1000)
            {
                return basePrice * 0.95;
            }
            else
            {
                return basePrice * 0.98;
            }
        }
    }
}";

            var selection = new EliteSelection();
            var crossover = new OrderedCrossover();
            var mutation = new ReverseSequenceMutation();
            var fitness = new MetricsFitness<CyclomaticComplexityMetrics>();
            var chromosome = new RefactoringChromosome(3, source);
            var population = new Population(20, 30, chromosome);

            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                Termination = new GenerationNumberTermination(50)
            };
            ga.Start();

            Assert.IsNotNull(ga.BestChromosome.Fitness);
            TestContext.WriteLine($"Best fitness is {ga.BestChromosome.Fitness} and source looks like: \n {ga.BestChromosome}");
        }
        
        [TestMethod]
        public void LinesOfCode()
        {
            const string source = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class Calculations
    {
        double CalculateTotal(int quantity, int itemPrice)
        {
            double basePrice = quantity * itemPrice;

            if (basePrice > 1000)
            {
                return basePrice * 0.95;
            }
            else
            {
                return basePrice * 0.98;
            }
        }
    }
}";

            var selection = new EliteSelection();
            var crossover = new OrderedCrossover();
            var mutation = new ReverseSequenceMutation();
            var fitness = new MetricsFitness<LinesOfCodeMetrics>();
            var chromosome = new RefactoringChromosome(3, source);
            var population = new Population(20, 30, chromosome);

            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                Termination = new GenerationNumberTermination(50)
            };
            ga.Start();

            Assert.IsNotNull(ga.BestChromosome.Fitness);
            TestContext.WriteLine($"Best fitness is {ga.BestChromosome.Fitness} and source looks like: \n {ga.BestChromosome}");
        }
        
        
        [TestMethod]
        public void MaintainabilityIndexMetrics()
        {
            const string source = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class Calculations
    {
        double CalculateTotal(int quantity, int itemPrice)
        {
            double basePrice = quantity * itemPrice;

            if (basePrice > 1000)
            {
                return basePrice * 0.95;
            }
            else
            {
                return basePrice * 0.98;
            }
        }
    }
}";

            var selection = new EliteSelection();
            var crossover = new OrderedCrossover();
            var mutation = new ReverseSequenceMutation();
            var fitness = new MetricsFitness<MaintainabilityIndexMetrics>();
            var chromosome = new RefactoringChromosome(5, source);
            var population = new Population(20, 30, chromosome);

            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                Termination = new GenerationNumberTermination(50)
            };
            ga.Start();

            Assert.IsNotNull(ga.BestChromosome.Fitness);
            TestContext.WriteLine($"Best fitness is {ga.BestChromosome.Fitness} and source looks like: \n {ga.BestChromosome}");
        }

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
    }
}
