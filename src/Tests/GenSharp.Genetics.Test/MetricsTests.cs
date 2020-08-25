using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GenSharp.Metrics.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenSharp.Genetics.Test
{
    [TestClass]
    public class MetricsTests
    {
        [TestMethod]
        public void CyclomaticComplexity_Successful()
        {
            const string source = @"
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

            var metrics = new CyclomaticComplexityMetrics(source);
            var value = metrics.Evaluate();

            Assert.IsTrue(value == 2);
        }
    }
}
