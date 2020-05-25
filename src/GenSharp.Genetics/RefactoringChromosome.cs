using GeneticSharp.Domain.Chromosomes;
using GenSharp.Refactorings.Analyzers.Helpers;

namespace GenSharp.Genetics
{
    public class RefactoringChromosome : ChromosomeBase
    {
        private readonly string _source;

        public RefactoringChromosome(int length, string source) : base(length)
        {
            _source = source;
        }

        public override Gene GenerateGene(int geneIndex)
        {
            var diagnostics = new DiagnosticsExtractor(_source).FromCode(geneIndex);
            return new Gene(diagnostics);
        }

        public override IChromosome CreateNew()
        {
            return new RefactoringChromosome(Length, _source);
        }
    }
}
