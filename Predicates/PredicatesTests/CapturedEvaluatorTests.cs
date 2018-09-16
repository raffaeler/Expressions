using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Predicates.CapturedEvaluator;
using PredicatesTests.Models;

namespace PredicatesTests
{
    [TestClass]
    public class CapturedEvaluatorTests
    {
        private static int _age;

        [TestMethod]
        public void Evaluator_EnumerableWithVariableCaptured()
        {
            var persons = Samples.GetSampleData();
            var filterDelegate = GetFilterByAgeFunc();
            var result = persons.Where(filterDelegate);

            _age = 20;
            var res1 = result.ToList();
            _age = 99;
            var res2 = result.ToList();

            Assert.AreNotEqual(res1.Count, res2.Count);
        }

        public static Func<Person, bool> GetFilterByAgeFunc()
        {
            return p => p.Age >= _age;
        }


        [TestMethod]
        public void Evaluator_QueryableWithConstant()
        {
            // no capture involved
            var persons = Samples.GetSampleData().AsQueryable();
            var filterExpression = GetFilterByAdult();

            var result = persons.Where(filterExpression);
            var res1 = result.ToList();

            Assert.AreEqual(res1.Count, 4);
        }

        public static Expression<Func<Person, bool>> GetFilterByAdult()
        {
            // age parameter is hardcoded
            return p => p.Age >= 18;
        }

        [TestMethod]
        public void Evaluator_QueryableWithParameterCaptured()
        {
            // parameter is captured in the expression but
            // its value is not reachable anymore
            var persons = Samples.GetSampleData().AsQueryable();
            var filterExpression = GetFilterByAgeParam(18);

            var result = persons.Where(filterExpression);
            var res1 = result.ToList();

            Assert.AreEqual(res1.Count, 4);
        }

        [TestMethod]
        public void Evaluator_QueryableWithVariableCaptured()
        {
            // a variable is captured in the expression
            // and changing its value, will impact on the result
            var persons = Samples.GetSampleData().AsQueryable();
            var filterExpression = GetFilterByAge();

            var result = persons.Where(filterExpression);

            _age = 20;
            var res1 = result.ToList();
            _age = 99;
            var res2 = result.ToList();

            Assert.AreNotEqual(res1.Count, res2.Count); // not equal!
        }

        [TestMethod]
        public void Evaluator_QueryableWithVariableEvaluated()
        {
            // a variable is captured in the expression
            // but we evaluate its value and replace it
            // in the expression with a constant value
            var persons = Samples.GetSampleData().AsQueryable();
            var filterExpression = GetFilterByAge();

            // evaluation of the captured variable
            var evaluatedFilterExpression = CapturedEvaluator.Eval(filterExpression);
            var result = persons.Where(
                (Expression<Func<Person, bool>>)evaluatedFilterExpression);

            _age = 20;
            var res1 = result.ToList();
            _age = 99;
            var res2 = result.ToList();

            Assert.AreEqual(res1.Count, res2.Count);    // equal!
        }

        [TestMethod]
        public void Evaluator_QueryableWithParameterEvaluated()
        {
            // a variable is captured in the expression
            // but we evaluate its value and replace it
            // in the expression with a constant value
            var persons = Samples.GetSampleData().AsQueryable();
            var filterExpression = GetFilterByAgeParam(18);

            // evaluation of the captured variable
            var evaluatedFilterExpression = CapturedEvaluator.Eval(filterExpression);
            var result = persons.Where(
                (Expression<Func<Person, bool>>)evaluatedFilterExpression);

            _age = 20;
            var res1 = result.ToList();
            _age = 99;
            var res2 = result.ToList();

            Assert.AreEqual(res1.Count, res2.Count);    // equal!
        }

        public static Expression<Func<Person, bool>> GetFilterByAge()
        {
            // age variable will be captured
            return p => p.Age >= _age;
        }

        public static Expression<Func<Person, bool>> GetFilterByAgeParam(int min)
        {
            // age parameter will be captured
            return p => p.Age >= min;
        }

    }
}
