using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Predicates.Injector;

namespace PredicatesTests
{
    [TestClass]
    public class Injector2Tests
    {
        private bool result;

        [TestMethod]
        public void Injector2TestWithMethodInfo()
        {
            var miTrue = this.GetType().GetMethod("PrintTrue");
            var miFalse = this.GetType().GetMethod("PrintFalse");

            var predicate = GetFilter();
            var injected = Injector2.Inject(predicate, this, miTrue, miFalse);
            var injectedDel = (Func<int, bool>)injected.Compile();

            var injectResultTrue = injectedDel(12);
            Assert.IsTrue(injectResultTrue);
            Assert.IsTrue(result);

            var injectResultFalse = injectedDel(5);
            Assert.IsFalse(injectResultFalse);
            Assert.IsFalse(result);
        }

        public void PrintTrue(int value)
        {
            result = true;
            Console.WriteLine($"{value} => True");
        }

        public void PrintFalse(int value)
        {
            result = false;
            Console.WriteLine($"{value} => False");
        }

        [TestMethod]
        public void Injector2TestWithLambdas()
        {
            var predicate = GetFilter();

            var injected = Injector2.Inject(predicate,
                x => PrintTrue(x),
                x => PrintFalse(x));
            var injectedDel = (Func<int, bool>)injected.Compile();

            var injectResultTrue = injectedDel(12);
            Assert.IsTrue(injectResultTrue);
            Assert.IsTrue(result);

            var injectResultFalse = injectedDel(5);
            Assert.IsFalse(injectResultFalse);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Injector2TestWithLambdas2()
        {
            var predicate = GetFilter();

            var injected = Injector2.Inject(predicate,
                x => PrintTrue(x),
                null);
            var injectedDel = (Func<int, bool>)injected.Compile();

            var injectResultTrue = injectedDel(12);
            Assert.IsTrue(injectResultTrue);
            Assert.IsTrue(result);

            var injectResultFalse = injectedDel(5);
            Assert.IsFalse(injectResultFalse);
        }

        public Expression<Func<int, bool>> GetFilter()
        {
            return p => p > 10 && p < 20;
        }
    }
}
