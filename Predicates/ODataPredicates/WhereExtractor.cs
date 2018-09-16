// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace ODataPredicates
{
    public class WhereExtractor<T> : ExpressionVisitor
    {
        public ParameterExpression Parameter { get; private set; }
        public Expression<Func<T, bool>> Lambda { get; private set; }

        public static WhereExtractor<T> Extract(Expression expression)
        {
            var instance = new WhereExtractor<T>();
            instance.Visit(expression);

            return instance;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "Where")
            {
                var arg1 = node.Arguments.Last();
                var oldPredicateExpression = (arg1 as UnaryExpression).Operand as LambdaExpression;// Expression<Func<T, bool>>;

                var newBody = base.Visit(oldPredicateExpression.Body);
                Lambda = Expression.Lambda<Func<T, bool>>(newBody, Parameter);

                return Lambda;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Type == typeof(T))
            {
                Parameter = node;
            }

            return base.VisitParameter(node);
        }

    }
}
