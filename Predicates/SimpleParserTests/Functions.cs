using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IAmRaf.SimpleParser;
using IAmRaf.SimpleParser.AbstractTree;

namespace SimpleParserTests
{
    [TestClass]
    public class Functions
    {
        [TestMethod]
        public void Numerical1()
        {
            var e = SimpleParser.Parse("x+ y",
                new Parameter(typeof(int), "x"),
                new Parameter(typeof(int), "y"));
            var f = e.Compile<Func<int, int, int>>();
            Assert.IsTrue(f(2, 2) == 4);
            Assert.IsFalse(f(2, 3) == 4);
        }

        [TestMethod]
        public void Numerical2()
        {
            var e = SimpleParser.Parse("x * y",
                new Parameter(typeof(double), "x"),
                new Parameter(typeof(double), "y"));
            var f = e.Compile<Func<double, double, double>>();
            Assert.IsTrue(f(3.5, 1.5) == 5.25);
            Assert.IsFalse(f(2, 2) == 4.1);
        }

        [TestMethod]
        public void Numerical3()
        {
            var e = SimpleParser.Parse("(4+x) * (y-3) + 1",
                new Parameter(typeof(double), "x"),
                new Parameter(typeof(double), "y"));
            var f = e.Compile<Func<double, double, double>>();
            Assert.IsTrue(f(1, 5) == 11);
        }

        [TestMethod]
        public void Numerical4()
        {
            var e = SimpleParser.Parse("x^2 + 2*x + 1",
                typeof(float),
                new Parameter(typeof(float), "x"));
            var f = e.Compile<Func<float, float>>();
            Assert.IsTrue(f(-1) == 0);
        }

        [TestMethod]
        public void Numerical5()
        {
            var e = SimpleParser.Parse("x^4 - 1",
                new Parameter(typeof(double), "x"));
            var f = e.Compile<Func<double, double>>();
            Assert.IsTrue(f(1) == 0);
        }
    }
}
