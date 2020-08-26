using System.Collections.Generic;

namespace GenSharp.Genetics
{
    public class ResultData
    {
        public List<double> Fitness { get; set; }

        public ResultData()
        {
            Fitness = new List<double>();
        }
    }
}
