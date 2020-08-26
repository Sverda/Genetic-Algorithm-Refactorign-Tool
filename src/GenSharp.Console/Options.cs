using CommandLine;
using GenSharp.Genetics;

namespace GenSharp.Console
{
    internal class Options
    {
        [Option('s', "source", Required = true, HelpText = "Input source code to be processed. ")]
        public string FilePath { get; set; }

        [Option('m', "metrics", Required = true, HelpText = "Metrics kind used in GA. ")]
        public MetricsKind Metrics { get; set; }
    }
}
