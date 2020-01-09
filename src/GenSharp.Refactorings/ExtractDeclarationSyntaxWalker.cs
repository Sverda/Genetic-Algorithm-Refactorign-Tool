using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
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
            Trace.WriteLine(extractedMethod.ToFullString());

            base.VisitVariableDeclaration(node);
        }

        private MethodDeclarationSyntax GenerateMethod(VariableDeclarationSyntax node)
        {
            var methodName = node.Variables.Where(v => !string.IsNullOrEmpty(v.Identifier.Text)).Select(v => v.Identifier.Text).Single();
            var returnExpression = node.DescendantNodes().OfType<BinaryExpressionSyntax>().Cast<ExpressionSyntax>().Single();
            var parameters = new List<ParameterSyntax>();
            var identifiers = returnExpression.DescendantTokens().Where(t => t.IsKind(SyntaxKind.IdentifierToken));
            foreach (var id in identifiers)
            {
                var compilation = CSharpCompilation
                    .Create("GenSharp.Walker")
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddSyntaxTrees(id.SyntaxTree);
                var model = compilation.GetSemanticModel(id.SyntaxTree);
                var idType = model.GetTypeInfo(id.Parent).Type;
                var type = SyntaxFactory.ParseTypeName(idType.Name);
                var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(id.ValueText)).WithType(type);
                parameters.Add(parameter);
            }

            var methodRoot = SyntaxFactory.MethodDeclaration(node.Type, methodName)
                .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(returnExpression)))
                .AddParameterListParameters(parameters.ToArray())
                .NormalizeWhitespace();

            return methodRoot;
        }
    }
}
