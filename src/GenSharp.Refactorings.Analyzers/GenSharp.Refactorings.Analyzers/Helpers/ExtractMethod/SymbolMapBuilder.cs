using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal class SymbolMapBuilder : SyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly TextSpan _span;
        private readonly Dictionary<ISymbol, List<SyntaxToken>> _symbolMap;
        private readonly CancellationToken _cancellationToken;

        public static Dictionary<ISymbol, List<SyntaxToken>> Build(
            SemanticModel semanticModel,
            SyntaxNode root,
            TextSpan span,
            CancellationToken cancellationToken)
        {
            var builder = new SymbolMapBuilder(semanticModel, span, cancellationToken);
            builder.Visit(root);

            return builder._symbolMap;
        }

        private SymbolMapBuilder(
            SemanticModel semanticModel,
            TextSpan span,
            CancellationToken cancellationToken)
            : base(SyntaxWalkerDepth.Token)
        {
            _semanticModel = semanticModel;
            _span = span;
            _symbolMap = new Dictionary<ISymbol, List<SyntaxToken>>();
            _cancellationToken = cancellationToken;
        }

        protected override void VisitToken(SyntaxToken token)
        {
            if (token.IsMissing || !_span.Contains(token.Span))
            {
                return;
            }

            var symbolInfo = _semanticModel.GetSymbolInfo(token.Parent, _cancellationToken);
            foreach (var sym in GetAllSymbols(symbolInfo))
            {
                var list = _symbolMap.GetOrAdd(sym, _ => new List<SyntaxToken>());
                list.Add(token);
            }
        }

        internal ImmutableArray<ISymbol> GetAllSymbols(SymbolInfo symbol)
        {
            if (symbol.Symbol != null)
            {
                return ImmutableArray.Create<ISymbol>(symbol.Symbol);
            }
            else
            {
                return symbol.CandidateSymbols;
            }
        }
    }
}
