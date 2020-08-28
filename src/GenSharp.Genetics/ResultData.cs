using System.Collections.Generic;
using System.Text;

namespace GenSharp.Genetics
{
    public class ResultData
    {
        public List<double> Fitness { get; set; }

        public string CsvFitness { get; set; }

        public string BestChromosomeSource { get; set; }

        public MetricsResults InitialFitness { get; set; }

        public MetricsResults FinalFitness { get; set; }

        public string InitialFinalCsv { get; set; }

        public ResultData()
        {
            Fitness = new List<double>();
        }

        public ResultData BuildCsv()
        {
            BuildFitnessCsv();
            BuildInitialFinalCsv();

            return this;
        }

        private void BuildFitnessCsv()
        {
            var csv = new StringBuilder();
            csv.AppendLine("Generation,Fitness");

            var generation = 1;
            foreach (var f in Fitness)
            {
                csv.AppendLine($"{generation},{f}");
                generation++;
            }

            CsvFitness = csv.ToString();
        }

        private void BuildInitialFinalCsv()
        {
            var csv = new StringBuilder();
            csv.AppendLine("CC,LoC,MI");
            csv.AppendLine(
                $"{InitialFitness.CyclomaticComplexity},{InitialFitness.LinesOfCode},{InitialFitness.MaintainabilityIndex}");
            csv.AppendLine(
                $"{FinalFitness.CyclomaticComplexity},{FinalFitness.LinesOfCode},{FinalFitness.MaintainabilityIndex}");

            InitialFinalCsv = csv.ToString();
        }
    }
}
