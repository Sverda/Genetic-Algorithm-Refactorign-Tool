using GenSharp.Genetics;
using System.IO;
using System.Threading.Tasks;
using CommandLine;

namespace GenSharp.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(RunOptions);
        }

        private static void RunOptions(Options opts)
        {
            RunOptionsAsync(opts).Wait();
        }

        private static async Task RunOptionsAsync(Options opts)
        {
            System.Console.WriteLine("Reading configuration. ");
            var source = await ReadSource(opts.FilePath);
            System.Console.WriteLine("Setting up the genetic algorithm. ");
            var runner = RunnerSetup(opts);
            System.Console.WriteLine("The genetic algorithm is running... ");
            var animation = new Spinner(10, 10);
            animation.Start();
            runner.Run(source);
            animation.Stop();
        }

        private static GeneticRunner RunnerSetup(Options opts)
        {
            var gaParams = new GeneticParameters
            {
                SequenceLength = 5,
                MinPopulation = 20,
                MaxPopulation = 100,
                Generations = 50,
                MetricsKind = opts.Metrics
            };
            var runner = new GeneticRunner(gaParams);
            return runner;
        }

        private static async Task<string> ReadSource(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
