﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal class SemanticDocument : SyntacticDocument
    {
        public readonly SemanticModel SemanticModel;

        private SemanticDocument(Document document, SourceText text, SyntaxTree tree, SyntaxNode root, SemanticModel semanticModel)
            : base(document, text, tree, root)
        {
            SemanticModel = semanticModel;
        }

        public static async Task<SemanticDocument> CreateAsync(Document document, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            return new SemanticDocument(document, text, root.SyntaxTree, root, model);
        }
    }
}
