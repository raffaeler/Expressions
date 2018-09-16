using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ODataPredicates;
using ODataPredicates.CapturedEvaluator;
using PredicatesTests.ODataModels;

namespace PredicatesTests
{
    [TestClass]
    public class ODataPredicateBuilderTests
    {
        [TestMethod]
        public void ODataWhere1()
        {
            var type = typeof(Family);
            var query = "Name eq 'Da Family'";
            var family = Samples.GetSampleData();

            ODataPredicateBuilder queryHelper = new ODataPredicateBuilder(typeof(Family));
            queryHelper.EvaluateLocals = true;

            var expression = queryHelper.GetExpressionFromString(typeof(Family), typeof(Family), query);
            //var evaluatedExpression = CapturedEvaluator.Eval(expression);

            var extractor = WhereExtractor<Family>.Extract(expression);
            var lambda = extractor.Lambda;
            var deleg = lambda.Compile();
            var result = family.Where(deleg).ToList();

            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ODataExpand1()
        {

            var dataSource = Samples.GetSampleData().AsQueryable();
            //var orderby = "Name desc";
            var expand = "Persons($filter=Id eq 2)";

            var expression = ODataApply<Family, Person>(dataSource, null, null, expand);
            var deleg = expression.Compile();
            var result = deleg.Invoke().ToList();

            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void ODataOrderBy()
        {
            var dataSource = Samples.GetSampleData().AsQueryable();
            var orderby = "Name desc";

            var expression = ODataApply<Family, Family>(dataSource, null, orderby, null);
            var deleg = expression.Compile();
            var result = deleg.Invoke().ToList();

            Assert.IsTrue(result.Count == 2);
        }

        public Expression<Func<IQueryable<T>>> ODataApply<T, K>(IQueryable<T> dataSource,
            string filter, string orderby, string selectExpand)
        {
            var queryHelper = new ODataPredicateBuilder(typeof(T), typeof(K));
            queryHelper.EvaluateLocals = true;

            var expression = queryHelper.GetExpressionFromString(typeof(T), typeof(K), filter, orderby, selectExpand);

            var lambda = DataSourceReplacementVisitor.Replace(expression, dataSource);
            return lambda;
        }

    }
}
