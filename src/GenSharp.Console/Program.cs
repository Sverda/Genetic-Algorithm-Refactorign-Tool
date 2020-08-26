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
                .ParseArguments<CommandLineOptions>(args)
                .WithParsed(RunOptions);
        }

        private static void RunOptions(CommandLineOptions commandLineOptions)
        {
            RunOptionsAsync(commandLineOptions).Wait();
        }

        private static async Task RunOptionsAsync(CommandLineOptions commandLineOptions)
        {
            string source = null;
            await Spinner.StartAsync("Reading configuration...", async spinner =>
            {
                source = await ReadSource(commandLineOptions.SourcePath);
            });

            GeneticRunner runner = null;
            Spinner.Start("Setting up the genetic algorithm...", spinner =>
            {
                runner = RunnerSetup(commandLineOptions, source);
            });
            
            Spinner.Start("The genetic algorithm is running...", spinner =>
            {
                runner.Run();
            });

            Spinner.Start("Saving results...", spinner =>
            {
                var data = runner.CollectResult();
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ResultData));
                using var writer = new StreamWriter(commandLineOptions.OutputPath);
                serializer.Serialize(writer, data);
            });
        }

        private static GeneticRunner RunnerSetup(CommandLineOptions commandLineOptions, string source)
        {
            var @params = new GeneticParameters
            {
                SequenceLength = 5,
                MinPopulation = 20,
                MaxPopulation = 100,
                Generations = 50,
                MetricsKind = commandLineOptions.ChoosenMetrics,
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
