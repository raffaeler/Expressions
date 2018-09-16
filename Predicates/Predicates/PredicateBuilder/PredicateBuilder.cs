// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Predicates.PredicateBuilder
{
    public class PredicateBuilder
    {
        public Expression<Func<T, bool>> CreateLambdaPredicate<T>(
            Expression body, ParameterExpression parameter)
        {
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public Expression And<T>(ParameterExpression parameter,
            params Expression<Func<T, bool>>[] conditions)
        {
            return Combine(ExpressionType.AndAlso, parameter, conditions);
        }

        public Expression And(params Expression[] conditions)
        {
            return AggregateWithPrevious(conditions, ExpressionType.AndAlso);
        }

        public Expression Or<T>(ParameterExpression parameter,
            params Expression<Func<T, bool>>[] conditions)
        {
            return Combine(ExpressionType.OrElse, parameter, conditions);
        }

        public Expression Or(params Expression[] conditions)
        {
            return AggregateWithPrevious(conditions, ExpressionType.OrElse);
        }

        public Expression Combine<T>(ExpressionType expressionType,
            ParameterExpression parameter,
            params Expression<Func<T, bool>>[] conditions)
        {
            var updatedConditionBodies = conditions
                .Select(c => ReplaceParameterVisitor
                    .GetBodyWithNewParameter<T>(c, parameter));

            var root = AggregateWithPrevious(updatedConditionBodies,
                expressionType);

            return root;
        }

        private Expression AggregateWithPrevious(
            IEnumerable<Expression> expressions,
            ExpressionType expressionType)
        {
            using (var it = expressions.GetEnumerator())
            {
                Expression first = null;
                while (it.MoveNext())
                {
                    if (first == null)
                    {
                        first = it.Current;
                    }
                    else
                    {
                        first = Expression.MakeBinary(expressionType,
                            first, it.Current);
                    }
                }

                return first;
            }
        }

        private IEnumerable<(Expression left, Expression right)> GetPairs(IEnumerable<Expression> expressions, Expression oddExpression)
        {
            using (var it = expressions.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var left = it.Current;
                    Expression right = oddExpression;
                    if (it.MoveNext())
                    {
                        right = it.Current;
                    }

                    yield return (left, right);
                }
            }
        }
    }
}
