using CommandLine;
using GenSharp.Genetics;
using Kurukuru;
using System.IO;
using System.Threading.Tasks;

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

        private static void RunOptions(Options options)
        {
            RunOptionsAsync(options).Wait();
        }

        private static async Task RunOptionsAsync(Options options)
        {
            await Spinner.StartAsync("Starting...", async spinner =>
            {
                spinner.Text = "Reading configuration...";
                var source = await ReadSource(options.FilePath);
                spinner.Text = "Setting up the genetic algorithm...";
                var runner = RunnerSetup(options, source);
                spinner.Text = "The genetic algorithm is running...";
                runner.Run();
                spinner.Text = "The genetic algorithm ended. ";
            });
        }

        private static GeneticRunner RunnerSetup(Options options, string source)
        {
            var gaParams = new GeneticParameters
            {
                SequenceLength = 5,
                MinPopulation = 20,
                MaxPopulation = 100,
                Generations = 50,
                MetricsKind = options.Metrics,
                Source = source
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
