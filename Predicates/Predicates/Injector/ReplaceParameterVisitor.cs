// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Predicates.Injector
{
    public class ReplaceParameter1Visitor : ExpressionVisitor
    {
        private ParameterExpression _source;
        private ParameterExpression _target;
        private ReplaceParameter1Visitor(ParameterExpression source,
            ParameterExpression target)
        {
            _source = source;
            _target = target;
        }

        public static Expression GetBodyWithNewParameter<T>(
            LambdaExpression expression,
            ParameterExpression parameter)
        {
            if (parameter.Type != typeof(T))
            {
                throw new ArgumentException($@"The parameter type {parameter.Type.Name} is not valid for the provided expression");
            }

            var visitor = new ReplaceParameter1Visitor(
                expression.Parameters.First(), parameter);
            return visitor.Visit(expression.Body);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (object.ReferenceEquals(node, _source))
            {
                return _target;
            }

            return base.VisitParameter(node);
        }
    }
}
