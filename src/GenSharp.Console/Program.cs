using CommandLine;
using GenSharp.Genetics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kurukuru;

namespace GenSharp.Console
{
    internal class Program
    {
        private static void Main(string[] args)
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
            await Spinner.StartAsync("Starting...", async spinner =>
            {
                spinner.Text = "Reading configuration...";
                var source = await ReadSource(opts.FilePath);
                spinner.Text = "Setting up the genetic algorithm...";
                var runner = RunnerSetup(opts);
                spinner.Text = "The genetic algorithm is running...";
                runner.Run(source);
                spinner.Text = "The genetic algorithm ended. ";
            });
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
