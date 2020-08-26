using System;
using GeneticSharp.Domain.Fitnesses;
using GenSharp.Metrics.Implementations;

namespace GenSharp.Genetics
{
    public enum MetricsKind
    {
        CyclomaticComplexity,
        LinesOfCode,
        MaintainabilityIndex
    }

    internal static class MetricsKindExtensions
    {
        public static IFitness MapFitness(this MetricsKind kind)
        {
            switch (kind)
            {
                case MetricsKind.CyclomaticComplexity:
                    return new MetricsFitness<CyclomaticComplexityMetrics>();

                case MetricsKind.LinesOfCode:
                    return new MetricsFitness<LinesOfCodeMetrics>();

                case MetricsKind.MaintainabilityIndex:
                    return new MetricsFitness<MaintainabilityIndexMetrics>();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
