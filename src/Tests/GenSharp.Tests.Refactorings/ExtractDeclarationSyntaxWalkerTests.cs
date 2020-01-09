using GenSharp.Refactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace GenSharp.Tests.Refactorings
{
    [TestClass]
    public class ExtractDeclarationSyntaxWalkerTests
    {
        [TestMethod]
        public void ExtractDeclarationWithTwoParameters()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            const string source = @"
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
            }";
            var tree = CSharpSyntaxTree.ParseText(source);

            new ExtractDeclarationSyntaxWalker().Visit(tree.GetRoot());

            var actual = tree.ToString();

            const string expected = @"
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

            double basePrice(int q, int i)
            {
                return q * i;
            }";

            Assert.AreEqual(expected, actual);
        }
    }
}
