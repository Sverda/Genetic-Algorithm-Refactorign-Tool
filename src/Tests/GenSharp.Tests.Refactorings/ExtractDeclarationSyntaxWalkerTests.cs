using GenSharp.Refactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenSharp.Tests.Refactorings
{
    [TestClass]
    public class ExtractDeclarationSyntaxWalkerTests
    {
        [TestMethod]
        public void ExtractDeclarationWithTwoParameters()
        {
            const string source = @"
            class Calculations
            {
                double CalculateTotal(int quantity, int itemPrice)
                {
                    double basePrice = quantity * itemPrice;

                    if (basePrice > 1000)
                    {
                        return basePrice * 0.95;
                    }
                    else
                    {
                        return basePrice * 0.98;
                    }
                }
            }";

            GetSemanticModel(source, out var tree, out var model);
            var result = new ExtractDeclarationSyntaxRewriter(model).Visit(tree.GetRoot());

            const string expected = @"
            class Calculations
            {
                double CalculateTotal(int quantity, int itemPrice)
                {
                    if (basePrice(quantity, itemPrice) > 1000)
                    {
                        return basePrice(quantity, itemPrice) * 0.95;
                    }
                    else
                    {
                        return basePrice(quantity, itemPrice) * 0.98;
                    }
                }

                double basePrice(int quantity, int itemPrice)
                {
                    return quantity * itemPrice;
                }
            }";

            Assert.AreEqual(expected, result.ToString());
        }

        private static void GetSemanticModel(string source, out SyntaxTree tree, out SemanticModel model)
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
