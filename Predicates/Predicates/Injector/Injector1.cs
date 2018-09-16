// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Predicates.Injector
{
    public static class Injector1
    {
        public static LambdaExpression Inject(LambdaExpression predicate,
            object instance, MethodInfo methodInfoTrue, MethodInfo methodInfoFalse)
        {
            if (predicate.ReturnType != typeof(bool) &&
                predicate.Parameters.Count != 1)
            {
                throw new ArgumentException("The predicate must be a Func<T, bool>");
            }

            var instanceExp = Expression.Constant(instance);
            Expression callTrue = methodInfoTrue == null ?
                Expression.Empty() :
                (Expression)Expression.Call(instanceExp, methodInfoTrue);

            Expression callFalse = methodInfoFalse == null ?
                Expression.Empty() :
                (Expression)Expression.Call(instanceExp, methodInfoFalse);

            return Inject(predicate, callTrue, callFalse);
        }

        public static LambdaExpression Inject(LambdaExpression predicate,
            Expression<Action> lambdaTrue, Expression<Action> lambdaFalse)
        {
            if (predicate.ReturnType != typeof(bool) &&
                predicate.Parameters.Count != 1)
            {
                throw new ArgumentException("The predicate must be a Func<T, bool>");
            }

            if ((lambdaTrue != null && lambdaTrue.Parameters.Count > 0) ||
                (lambdaFalse != null && lambdaFalse.Parameters.Count > 0))
            {
                throw new ArgumentException("Lambdas cannot accept input parameters");
            }

            Expression callTrue = lambdaTrue == null ?
                Expression.Empty() : lambdaTrue.Body;

            Expression callFalse = lambdaFalse == null ?
                Expression.Empty() : lambdaFalse.Body;

            return Inject(predicate, callTrue, callFalse);
        }

        private static LambdaExpression Inject(LambdaExpression predicate,
            Expression ifTrue, Expression ifFalse)
        {
            var blockExpressions = new List<Expression>();
            var variable = Expression.Variable(typeof(bool), "res");
            var assign = Expression.Assign(variable, predicate.Body);
            blockExpressions.Add(assign);

            var ifExpression = Expression.IfThenElse(variable,
                ifTrue, ifFalse);
            blockExpressions.Add(ifExpression);

            blockExpressions.Add(variable); // return value!

            var block = Expression.Block(
                new ParameterExpression[] { variable, }, blockExpressions);
            var lambda = Expression.Lambda(block, predicate.Parameters);
            return lambda;
        }
    }
}
