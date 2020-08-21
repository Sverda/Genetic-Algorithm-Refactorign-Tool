using GenSharp.Refactorings.Analyzers.Analyzers;
using GenSharp.Refactorings.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace GenSharp.Refactorings.Analyzers.Test
{
    [TestClass]
    public class ExtractDeclarationTests : CodeFixVerifier
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
                Id = DiagnosticIdentifiers.ExtractStatement,
                Message = DiagnosticDescriptors.ExtractStatement.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 15, 20)
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            const string fixtest = @"
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
    }
}";
            VerifyCSharpFix(test, fixtest);
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
                Id = DiagnosticIdentifiers.ExtractStatement,
                Message = DiagnosticDescriptors.ExtractStatement.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 15, 20)
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            const string fixtest = @"
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
            if (basePrice(quantity, itemPrice) > 1000)
            {
                return basePrice(quantity, itemPrice) * 0.95;
            }
            else
            {
                return basePrice(quantity, itemPrice) * 0.98;
            }
        }
        double basePrice(int quantity, double itemPrice)
        {
            return quantity * itemPrice;
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
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
                Id = DiagnosticIdentifiers.ExtractStatement,
                Message = DiagnosticDescriptors.ExtractStatement.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 18, 20)
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            const string fixtest = @"
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
    }
}";
            VerifyCSharpFix(test, fixtest);
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
                Id = DiagnosticIdentifiers.ExtractStatement,
                Message = DiagnosticDescriptors.ExtractStatement.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 17, 20)
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            const string fixtest = @"
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
            if (basePrice() > 1000)
            {
                return basePrice() * 0.95;
            }
            else
            {
                return basePrice() * 0.98;
            }
        }
        double basePrice()
        {
            return _quantity * _itemPrice;
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
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
                Id = DiagnosticIdentifiers.ExtractStatement,
                Message = DiagnosticDescriptors.ExtractStatement.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 18, 20)
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            const string fixtest = @"
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
            if (basePrice(itemPrice) > 1000)
            {
                return basePrice(itemPrice) * 0.95;
            }
            else
            {
                return basePrice(itemPrice) * 0.98;
            }
        }

        double basePrice(int itemPrice)
        {
            return _quantity * itemPrice;
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ExtractStatementCodeFix();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExtractStatementAnalyzer();
        }
    }
}
