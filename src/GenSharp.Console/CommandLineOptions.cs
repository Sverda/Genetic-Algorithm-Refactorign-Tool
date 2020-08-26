using CommandLine;
using GenSharp.Genetics;

namespace GenSharp.Console
{
    internal class CommandLineOptions
    {
        [Option('s', "source", Required = true, HelpText = "Input source code to be processed. ")]
        public string SourcePath { get; set; }

        [Option('m', "metrics", Required = true, HelpText = "Metrics kind used in GA. ")]
        public MetricsKind ChoosenMetrics { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output path for GA's result")]
        public string OutputPath { get; set; }
    }
}
