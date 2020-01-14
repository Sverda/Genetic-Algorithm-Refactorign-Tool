using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GenSharp.Refactorings
{
    public static class StringExtensions
    {
        public static void GetSemanticModel(this string source, out SyntaxTree tree, out SemanticModel model)
        {
            tree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation
                .Create("GenSharp.Tests.Refactorings")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);
            model = compilation.GetSemanticModel(tree);
        }
    }
}
