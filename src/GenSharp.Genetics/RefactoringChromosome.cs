using System.Linq;
using GeneticSharp.Domain.Chromosomes;
using GenSharp.Refactorings.Analyzers.Helpers;
using Microsoft.CodeAnalysis;

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
            return new RefactoringChromosome(Length, ApplyFixes());
        }

        public override string ToString() => ApplyFixes();

        internal string ApplyFixes()
        {
            var sequence = GetGenes().Select(g => g.Value).Cast<Diagnostic>();
            var newSource = CodeFixApplier.ComputeCodeFixes(Source, sequence);
            return newSource;
        }
    }
}
