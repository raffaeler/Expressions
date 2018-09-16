using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IAmRaf.SimpleParser;
using IAmRaf.SimpleParser.AbstractTree;

namespace SimpleParserTests
{
    [TestClass]
    public class Predicates
    {
        [TestMethod]
        public void Predicate1()
        {
            var e = SimpleParser.Parse("x >=1", Parameter.Create<int>("x"));
            var f = e.Compile<Func<int, bool>>();
            Assert.IsTrue(f(1));
            Assert.IsFalse(f(-3));

        }

        [TestMethod]
        public void Predicate2()
        {
            var e = SimpleParser.Parse("x+4<=2", Parameter.Create<int>("x"));
            var f = e.Compile<Func<int, bool>>();
            Assert.IsFalse(f(1));
            Assert.IsTrue(f(-10));
        }

        [TestMethod]
        public void Predicate3()
        {
            var e = SimpleParser.Parse("x + 5.2 > 9.2", Parameter.Create<double>("x"));
            var f = e.Compile<Func<double, bool>>();
            Assert.IsFalse(f(4));
            Assert.IsTrue(f(4.1));
        }

        [TestMethod]
        public void Predicate4()
        {
            var e = SimpleParser.Parse("2+(x + 5)* 3 >1", Parameter.Create<int>("x"));
            var f = e.Compile<Func<int, bool>>();
            Assert.IsFalse(f(-6));
            Assert.IsTrue(f(5));
        }

        [TestMethod]
        public void Predicate5()
        {
            var e = SimpleParser.Parse("x == \"raf\"", Parameter.Create<string>("x"));
            var f = e.Compile<Func<string, bool>>();
            Assert.IsFalse(f("Raf"));
            Assert.IsTrue(f("raf"));
        }

        [TestMethod]
        public void Predicate6()
        {
            var e = SimpleParser.Parse("x >=1 && x<5", Parameter.Create<int>("x"));
            var f = e.Compile<Func<int, bool>>();
            Assert.IsTrue(f(3));
            Assert.IsFalse(f(6));
        }

        [TestMethod]
        public void Predicate7()
        {
            var e = SimpleParser.Parse("x >=1 && x<5 && y>0 && y<10",
                Parameter.Create<int>("x"),
                Parameter.Create<int>("y"));
            var f = e.Compile<Func<int, int, bool>>();
            Assert.IsTrue(f(3, 3));
            Assert.IsFalse(f(6, 3));
            Assert.IsFalse(f(3, 10));
        }

        [TestMethod]
        public void Compare1()
        {
            var e1 = SimpleParser.Parse("x >= 10", Parameter.Create<int>("x"));

            var left = Expression.Parameter(typeof(int), "x");
            var right = Expression.Constant(10, typeof(int));
            var binary = Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, left, right);

            // building the predicate
            var lambda = Expression.Lambda<Func<int, bool>>(binary, left);

            Assert.AreEqual(e1.Expression.ToString(), lambda.ToString());
        }

    }
}
