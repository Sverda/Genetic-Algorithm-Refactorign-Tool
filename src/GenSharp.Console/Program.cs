using GenSharp.Metrics.Abstractions;
using GenSharp.Metrics.Implementations;

namespace GenSharp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            IEvaluateMetric metrics = new MaintainabilityIndexMetrics();
            System.Console.WriteLine(metrics.Evaluate());
        }
    }
}
