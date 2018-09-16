using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Predicates.Injector;

namespace PredicatesTests
{
    [TestClass]
    public class InjectorTests
    {
        private bool result;

        [TestMethod]
        public void Injector1TestWithMethodInfo()
        {
            var miTrue = this.GetType().GetMethod("PrintTrue");
            var miFalse = this.GetType().GetMethod("PrintFalse");

            var predicate = GetFilter();
            var injected = Injector1.Inject(predicate, this, miTrue, miFalse);
            var injectedDel = (Func<int, bool>)injected.Compile();

            var injectResultTrue = injectedDel(12);
            Assert.IsTrue(injectResultTrue);
            Assert.IsTrue(result);

            var injectResultFalse = injectedDel(5);
            Assert.IsFalse(injectResultFalse);
            Assert.IsFalse(result);
        }

        public void PrintTrue()
        {
            result = true;
            Console.WriteLine("True");
        }

        public void PrintFalse()
        {
            result = false;
            Console.WriteLine("False");
        }

        [TestMethod]
        public void Injector1TestWithLambdas()
        {
            var predicate = GetFilter();

            var injected = Injector1.Inject(predicate,
                () => PrintTrue(),
                () => PrintFalse());
            var injectedDel = (Func<int, bool>)injected.Compile();

            var injectResultTrue = injectedDel(12);
            Assert.IsTrue(injectResultTrue);
            Assert.IsTrue(result);

            var injectResultFalse = injectedDel(5);
            Assert.IsFalse(injectResultFalse);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Injector1TestWithLambdas2()
        {
            var predicate = GetFilter();

            var injected = Injector1.Inject(predicate,
                () => PrintTrue(),
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
