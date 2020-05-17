using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GenSharp.Refactorings.Analyzers.Helpers
{
    internal class ExtractedStatementModel
    {
        public SyntaxNode TargetStatement { get; set; }

        public MethodDeclarationSyntax Method { get; set; }

        public ExpressionStatementSyntax Call { get; set; }
    }
}
