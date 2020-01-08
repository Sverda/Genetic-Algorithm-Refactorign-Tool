using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace GenSharp.Refactorings
{
    public class ExtractDeclarationSyntaxWalker : CSharpSyntaxWalker
    {
        public ExtractDeclarationSyntaxWalker() : base(SyntaxWalkerDepth.Node)
        {
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var extractedMethod = GenerateMethod(node);

            base.VisitVariableDeclaration(node);
        }

        private MethodDeclarationSyntax GenerateMethod(VariableDeclarationSyntax node)
        {
            var methodName = node.Variables.Where(v => !string.IsNullOrEmpty(v.Identifier.Text)).Select(v => v.Identifier.Text).Single();
            var returnExpression = node.DescendantNodes().OfType<BinaryExpressionSyntax>().Cast<ExpressionSyntax>().Single();
            var parameters = new SeparatedSyntaxList<ParameterSyntax>();
            var identifiers = returnExpression.DescendantTokens().Where(t => t.IsKind(SyntaxKind.IdentifierToken));

            var methodRoot = SyntaxFactory.MethodDeclaration(node.Type, methodName)
                .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(returnExpression)))
                .WithParameterList(SyntaxFactory.ParameterList(parameters))
                .NormalizeWhitespace();

            return methodRoot;
        }
    }
}
