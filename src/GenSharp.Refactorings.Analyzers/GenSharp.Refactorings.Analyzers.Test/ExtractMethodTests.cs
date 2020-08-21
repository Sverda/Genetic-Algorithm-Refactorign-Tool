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
    public class ExtractMethodTests : CodeFixVerifier
    {
        [TestMethod]
        public void ExtractMethod1()
        {
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

            const string fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
class Program
{
    void Test(string[] args)
    {
        NewMethod();
    }

    private static void TestExtract1()
    {
        int i;
        i = 10;
    }
}";
            VerifyCSharpFix(test, fixtest);
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
