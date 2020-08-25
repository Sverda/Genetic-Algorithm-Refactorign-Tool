using GeneticSharp.Domain.Chromosomes;
using GenSharp.Refactorings.Analyzers.Helpers;

namespace GenSharp.Genetics
{
    public class RefactoringChromosome : ChromosomeBase
    {
        public string Source { get; }

        public RefactoringChromosome(int sequenceLength, string source) : base(sequenceLength)
        {
            Source = source;

            CreateGenes();
        }

        public override Gene GenerateGene(int geneIndex)
        {
            var diagnostics = new DiagnosticsExtractor(Source).FromCode(geneIndex);
            return new Gene(diagnostics);
        }

        public override IChromosome CreateNew()
        {
            return new RefactoringChromosome(Length, Source);
        }
    }
}
