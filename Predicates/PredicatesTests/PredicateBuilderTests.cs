using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Predicates.PredicateBuilder;
using PredicatesTests.Models;

namespace PredicatesTests
{
    [TestClass]
    public class PredicateBuilderTests
    {
        [TestMethod]
        public void PredicateBuilderTest1()
        {
            var isAnd = true;
            int age = 22;
            string name = "Daniel";

            var persons = Samples.GetSampleData();
            var builder = new PredicateBuilder();
            var parameter = Expression.Parameter(typeof(Person), "common");

            Expression condition;
            if (isAnd)
            {
                condition = builder.And<Person>(parameter,
                    p => p.Name == name,
                    p => p.Age == age);
            }
            else
            {
                condition = builder.Or<Person>(parameter,
                    p => p.Name == name,
                    p => p.Age == age);
            }

            var lambda = builder.CreateLambdaPredicate<Person>(
                condition, parameter);
            var predicate = lambda.Compile();
            var filtered = persons.Where(predicate).ToList();

            Assert.IsTrue(filtered.Count == 1);
        }

        [TestMethod]
        public void PredicateBuilderTest2()
        {
            var persons = Samples.GetSampleData();

            var builder = new PredicateBuilder();
            var parameter = Expression.Parameter(typeof(Person), "common");
            var condition1 = builder.And<Person>(parameter,
                p => p.Age > 15,
                p => p.Description.StartsWith('r'),
                p => p.Name.StartsWith('D'));
            var condition2 = builder.Or<Person>(parameter,
                p => p.Age > 10,
                p => p.Name.StartsWith('L'));

            var finalCondition = builder.And(condition1, condition2);
            var lambda = builder.CreateLambdaPredicate<Person>(
                finalCondition, parameter);
            var predicate = lambda.Compile();
            var filtered = persons.Where(predicate).ToList();

            Assert.IsTrue(filtered.Count == 1);
        }
    }
}
