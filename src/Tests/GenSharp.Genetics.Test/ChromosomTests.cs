using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenSharp.Genetics.Test
{
    [TestClass]
    public class ChromosomTests
    {
        [TestMethod]
        public void CreateGene_Successful()
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
            var factory = new RefactoringChromosome(3, source);
            var chromosome = factory.GenerateGene(0);
            Assert.IsNotNull(chromosome);
        }

        [TestMethod]
        public void CreateGene_OutOfBound()
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
            var factory = new RefactoringChromosome(2, source);
            var chromosome = factory.GenerateGene(1);
            Assert.IsNotNull(chromosome);
        }
    }
}
