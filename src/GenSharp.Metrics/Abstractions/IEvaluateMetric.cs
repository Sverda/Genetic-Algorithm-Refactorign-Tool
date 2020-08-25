namespace GenSharp.Metrics.Abstractions
{
    public interface IEvaluateMetric
    {
        IEvaluateMetric SetSource(string source);

        double Evaluate();
    }
}
