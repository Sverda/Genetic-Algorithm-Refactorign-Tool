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
            string source = null;
            await Spinner.StartAsync("Reading configuration...", async spinner =>
            {
                source = await ReadSource(options.FilePath);
            });

            GeneticRunner runner = null;
            Spinner.Start("Setting up the genetic algorithm...", spinner =>
            {
                runner = RunnerSetup(options, source);
            });
            
            Spinner.Start("The genetic algorithm is running...", spinner =>
            {
                runner.Run();
            });
        }

        private static GeneticRunner RunnerSetup(Options options, string source)
        {
            var @params = new GeneticParameters
            {
                SequenceLength = 5,
                MinPopulation = 20,
                MaxPopulation = 100,
                Generations = 50,
                MetricsKind = options.Metrics,
                Source = source
            };
            var runner = new GeneticRunner(@params);
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
