namespace GenSharp.Genetics
{
    public class GeneticParameters
    {
        public int SequenceLength { get; set; }
        public int MinPopulation { get; set; }
        public int MaxPopulation { get; set; }
        public int Generations { get; set; }
        public MetricsKind MetricsKind { get; set; }
    }
}
