using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace GenSharp.Refactorings.Analyzers.Test
{
    [TestClass]
    public class ExtractDeclarationAnalyzerTests : CodeFixVerifier
    {
        [TestMethod]
        public void SameTypesArguments()
        {
            const string test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
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
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "GenSharp",
                Message = "Variable declaration with name basePrice can be extracted",
                Severity = DiagnosticSeverity.Info,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 15, 17)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new GenSharpRefactoringsAnalyzersCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new GenSharpRefactoringsAnalyzersAnalyzer();
        }
    }
}
