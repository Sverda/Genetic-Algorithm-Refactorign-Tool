using GenSharp.Refactorings.Analyzers.Analyzers;
using GenSharp.Refactorings.Analyzers.CodeFixes;
using GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace GenSharp.Refactorings.Analyzers.Test
{
    [TestClass]
    public class ExtractMethodTests : CodeFixVerifier
    {
        [TestMethod]
        public void ExtractMethod1_1()
        {
            SelectionResultConfiguration.Set(0, 2);

            const string test = @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        int i;
        i = 10;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.ExtractMethod,
                Message = DiagnosticDescriptors.ExtractMethod.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 5)
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtests = new[]
            {
                @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        Test_ExtractedMethod();
    }

    private void Test_ExtractedMethod()
    {
        int i;
        i = 10;
    }
}"
            };

            VerifyCSharpFix(test, fixtests, null, true);
        }

        [TestMethod]
        public void ExtractMethod1_2()
        {
            SelectionResultConfiguration.Set(0, 1);

            const string test = @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        int i;
        i = 10;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.ExtractMethod,
                Message = DiagnosticDescriptors.ExtractMethod.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 5)
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtests = new[]
            {
                @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        var i = Test_ExtractedMethod();
        i = 10;
    }

    private int Test_ExtractedMethod()
    {
        int i;
        return i;
    }
}"
            };

            VerifyCSharpFix(test, fixtests, null, true);
        }

        [TestMethod]
        public void ExtractMethod2_1()
        {
            SelectionResultConfiguration.Set(0, 3);

            const string test = @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        int i;
        i = 10;
        i = 20;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.ExtractMethod,
                Message = DiagnosticDescriptors.ExtractMethod.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 5)
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtests = new[]
            {
                @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        Test_ExtractedMethod();
    }

    private void Test_ExtractedMethod()
    {
        int i;
        i = 10;
        i = 20;
    }
}"
            };

            VerifyCSharpFix(test, fixtests, null, true);
        }

        [TestMethod]
        public void ExtractMethod2_2()
        {
            SelectionResultConfiguration.Set(0, 1);

            const string test = @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        int i;
        i = 10;
        i = 20;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.ExtractMethod,
                Message = DiagnosticDescriptors.ExtractMethod.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 5)
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtests = new[]
            {
                @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        var i = Test_ExtractedMethod();
        i = 10;
        i = 20;
    }

    private int Test_ExtractedMethod()
    {
        int i;
        return i;
    }
}"
            };

            VerifyCSharpFix(test, fixtests, null, true);
        }

        [TestMethod]
        public void ExtractMethod3()
        {
            SelectionResultConfiguration.Set(1, 1);

            const string test = @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        int i = 10;
        int i2 = i;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.ExtractMethod,
                Message = DiagnosticDescriptors.ExtractMethod.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 5)
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtests = new[]
            {
                @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        int i = 10;
        Test_ExtractedMethod(i);
    }

    private void Test_ExtractedMethod(int i)
    {
        int i2 = i;
    }
}"
            };

            VerifyCSharpFix(test, fixtests, null, true);
        }

        [TestMethod]
        public void ExtractMethod4()
        {
            SelectionResultConfiguration.Set(2, 1);

            const string test = @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        int i = 10;
        int i2 = i;
        i2 = i;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = DiagnosticIdentifiers.ExtractMethod,
                Message = DiagnosticDescriptors.ExtractMethod.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Hidden,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 7, 5)
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtests = new[]
            {
                @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        int i = 10;
        int i2 = i;
        i2 = Test_ExtractedMethod(i);
    }

    private static int Test_ExtractedMethod(int i)
    {
        return i;
    }
}"
            };

            VerifyCSharpFix(test, fixtests, null, true);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExtractMethodAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ExtractMethodCodeFix();
        }
    }
}
