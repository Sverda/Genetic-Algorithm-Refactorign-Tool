using GenSharp.Refactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenSharp.Tests.Refactorings
{
    [TestClass]
    public class ExtractDeclarationRefactoring
    {
        [TestMethod]
        public void ExtractedMethodHas_SameTypesArguments()
        {
            const string source =
@"
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

            source.GetSemanticModel(out var tree, out var model);
            var result = new ExtractDeclarationSyntaxRewriter(model)
                .Visit(tree.GetRoot())
                .NormalizeWhitespace();

            const string expected =
@"class Calculations
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

        [TestMethod]
        public void ExtractedMethodHas_DifferentTypesArguments()
        {
            const string source =
                @"
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
}";

            source.GetSemanticModel(out var tree, out var model);
            var result = new ExtractDeclarationSyntaxRewriter(model)
                .Visit(tree.GetRoot())
                .NormalizeWhitespace();

            const string expected =
                @"class Calculations
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
}";

            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod]
        public void ExtractedMethodHas_LocalVariablesArguments()
        {
            const string source =
                @"
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
}";

            source.GetSemanticModel(out var tree, out var model);
            var result = new ExtractDeclarationSyntaxRewriter(model)
                .Visit(tree.GetRoot())
                .NormalizeWhitespace();

            const string expected =
                @"class Calculations
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
}";

            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod]
        public void ExtractedMethodHas_FieldsArguments()
        {
            const string source =
                @"
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
}";

            source.GetSemanticModel(out var tree, out var model);
            var result = new ExtractDeclarationSyntaxRewriter(model)
                .Visit(tree.GetRoot())
                .NormalizeWhitespace();

            const string expected =
                @"class Calculations
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
}";

            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod]
        public void ExtractedMethodHas_LocalVariableAndFieldArgument()
        {
            const string source =
                @"
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
}";

            source.GetSemanticModel(out var tree, out var model);
            var result = new ExtractDeclarationSyntaxRewriter(model)
                .Visit(tree.GetRoot())
                .NormalizeWhitespace();

            const string expected =
                @"class Calculations
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
}";

            Assert.AreEqual(expected, result.ToString());
        }
    }
}
