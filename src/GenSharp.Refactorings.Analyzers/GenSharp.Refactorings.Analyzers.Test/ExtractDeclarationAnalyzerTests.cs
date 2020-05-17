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

        [TestMethod]
        public void DifferentTypesArguments()
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
            double CalculateTotal(int quantity, double itemPrice)
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

        [TestMethod]
        public void LocalVariablesArguments()
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
            double CalculateTotal()
            {
                int quantity = 1;
                int itemPrice = 2;

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
                    new DiagnosticResultLocation("Test0.cs", 18, 17)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void FieldsArguments()
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
            int _quantity = 1;
            int _itemPrice = 2;
            double CalculateTotal()
            {
                double basePrice = _quantity * _itemPrice;

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
                    new DiagnosticResultLocation("Test0.cs", 17, 17)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void LocalVariableAndFieldArgument()
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
            int _quantity = 1;
            double CalculateTotal()
            {
                int itemPrice = 2;

                double basePrice = _quantity * itemPrice;

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
                    new DiagnosticResultLocation("Test0.cs", 18, 17)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new GenSharpCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExtractStatementAnalyzer();
        }
    }
}
