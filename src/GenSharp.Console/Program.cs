using GenSharp.Genetics;
using System.IO;

namespace GenSharp.Console
{
    internal class Program
    {
        private static void Main(string filePath, MetricsKind metrics)
        {
            var source = ReadSource(filePath);
            var gaParams = new GeneticParameters
            {
                SequenceLength = 5,
                MinPopulation = 20,
                MaxPopulation = 100,
                Generations = 50,
                MetricsKind = metrics
            };
            var runner = new GeneticRunner(gaParams);
            runner.Run(source);
        }

        private static string ReadSource(string filePath)
        {
            return File.ReadAllText(filePath);
        }
    }
}
