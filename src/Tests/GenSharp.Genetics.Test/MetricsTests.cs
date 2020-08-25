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

            var metrics = new CyclomaticComplexityMetrics()
                .SetSource(source);
            var value = metrics.Evaluate();

            Assert.AreEqual(2, value);
        }

        [TestMethod]
        public void MaintainabilityIndex_Successful()
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

            var metrics = new MaintainabilityIndexMetrics()
                .SetSource(source);
            var value = metrics.Evaluate();

            Assert.AreEqual(77, value);
        }

        [TestMethod]
        public void LinesOfCode_Successful()
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

            var metrics = new LinesOfCodeMetrics()
                .SetSource(source);
            var value = metrics.Evaluate();

            Assert.AreEqual(4, value);
        }
    }
}
