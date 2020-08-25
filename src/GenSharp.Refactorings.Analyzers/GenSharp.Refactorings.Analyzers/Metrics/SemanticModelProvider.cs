using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;

namespace GenSharp.Refactorings.Analyzers.Metrics
{
    internal sealed class SemanticModelProvider
    {
        private readonly ConcurrentDictionary<SyntaxTree, SemanticModel> _semanticModelMap;
        public SemanticModelProvider(Compilation compilation)
        {
            Compilation = compilation;
            _semanticModelMap = new ConcurrentDictionary<SyntaxTree, SemanticModel>();
        }

        public Compilation Compilation { get; }

        public SemanticModel GetSemanticModel(SyntaxNode node)
            => _semanticModelMap.GetOrAdd(node.SyntaxTree, tree => Compilation.GetSemanticModel(node.SyntaxTree));
    }
}
