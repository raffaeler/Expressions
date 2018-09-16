using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IAmRaf.SimpleParser;
using IAmRaf.SimpleParser.AbstractTree;

namespace SimpleParserTests
{
    [TestClass]
    public class MethodCalls
    {
        [TestMethod]
        public void FuncCalls1()
        {
            var e = SimpleParser.Parse("x + SUM(x- 1, 3, 2) - 2",
                new Parameter(typeof(double), "x"));
            var f = e.Compile<Func<double, double>>();
            Assert.IsTrue(f(-1) == 0);
        }

        [TestMethod]
        public void FuncCalls2()
        {
            var e = SimpleParser.Parse("AVG(1,2,3,4,5,5,6,7,8,9,)");
            var f = e.Compile<Func<double>>();
            Assert.IsTrue(f() == 5);
        }

        [TestMethod]
        public void FuncCalls3()
        {
            var e = SimpleParser.Parse("POW(2, 3)");
            var f = e.Compile<Func<double>>();
            Assert.IsTrue(f() == 8);
        }
    }
}
