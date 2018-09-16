// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ODataPredicates
{
    public class DataSourceReplacementVisitor : ExpressionVisitor
    {
        private Type _oldSourceType;
        private object _newSource;
        private DataSourceReplacementVisitor(Type oldSourceType, object newSource)
        {
            _oldSourceType = oldSourceType;
            _newSource = newSource;
        }

        public static Expression<Func<IQueryable<T>>> Replace<T>(Expression expression, IQueryable<T> dataSource)
        {
            var visitor = new DataSourceReplacementVisitor(typeof(T), dataSource);
            var methodCallExpression = visitor.Visit(expression);
            return Expression.Lambda<Func<IQueryable<T>>>(methodCallExpression);
        }

        public override Expression Visit(Expression node)
        {
            var expression = base.Visit(node);
            return expression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if ((node == null && node.Method == null) || (node != null && node.Arguments.Count == 0))
                return base.VisitMethodCall(node);

            var queryable = node.Arguments[0].Type;
            var otherArguments = node.Arguments.Skip(1).ToList();

            if (typeof(IQueryable).IsAssignableFrom(queryable) && queryable.IsGenericType)
            {
                var queryableArgument = queryable.GetGenericArguments().First();
                if (queryableArgument == _oldSourceType)
                {
                    var newDataSource = Expression.Constant(_newSource);
                    var argumentsList = new List<Expression>();
                    argumentsList.Add(newDataSource);
                    if (otherArguments.Count > 0)
                    {
                        argumentsList.AddRange(otherArguments);
                    }

                    var method = node.Method;
                    var newExpression = Expression.Call(method, argumentsList);
                    return newExpression;
                }
            }

            return base.VisitMethodCall(node);
        }
    }
}
