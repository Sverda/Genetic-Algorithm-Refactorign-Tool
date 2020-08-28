using System.Collections.Generic;

namespace GenSharp.Genetics
{
    public class ResultData
    {
        public List<double> Fitness { get; set; }

        public string BestChromosomeSource { get; set; }

        public MetricsResults InitialFitness { get; set; }

        public MetricsResults FinalFitness { get; set; }

        public ResultData()
        {
            Fitness = new List<double>();
        }
    }
}
