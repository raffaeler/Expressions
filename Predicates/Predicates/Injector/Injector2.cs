// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Predicates.Injector
{
    public static class Injector2
    {
        public static Expression<Func<T, bool>> Inject<T>(Expression<Func<T, bool>> predicate,
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
                (Expression)Expression.Call(instanceExp, methodInfoTrue, predicate.Parameters);

            Expression callFalse = methodInfoFalse == null ?
                Expression.Empty() :
                (Expression)Expression.Call(instanceExp, methodInfoFalse, predicate.Parameters);

            return Inject(predicate, callTrue, callFalse);
        }

        public static Expression<Func<T, bool>> Inject<T>(Expression<Func<T, bool>> predicate,
            Expression<Action<T>> lambdaTrue, Expression<Action<T>> lambdaFalse)
        {
            if (predicate.ReturnType != typeof(bool) &&
                predicate.Parameters.Count != 1)
            {
                throw new ArgumentException("The predicate must be a Func<T, bool>");
            }

            if ((lambdaTrue != null && lambdaTrue.Parameters.Count != predicate.Parameters.Count) ||
                (lambdaFalse != null && lambdaFalse.Parameters.Count != predicate.Parameters.Count))
            {
                throw new ArgumentException("Lambdas cannot accept input parameters");
            }

            var inputParameter = predicate.Parameters.Single();

            if (lambdaTrue != null)
            {
                lambdaTrue = Expression.Lambda<Action<T>>(
                    ReplaceParameter1Visitor.GetBodyWithNewParameter<T>(
                    lambdaTrue, inputParameter), inputParameter);
            }

            if (lambdaFalse != null)
            {
                lambdaFalse = Expression.Lambda<Action<T>>(
                    ReplaceParameter1Visitor.GetBodyWithNewParameter<T>(
                    lambdaFalse, inputParameter), inputParameter);
            }

            Expression callTrue = lambdaTrue == null ?
                Expression.Empty() : lambdaTrue.Body;

            Expression callFalse = lambdaFalse == null ?
                Expression.Empty() : lambdaFalse.Body;

            return Inject(predicate, callTrue, callFalse);
        }

        private static Expression<Func<T, bool>> Inject<T>(Expression<Func<T, bool>> predicate,
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
            var lambda = Expression.Lambda<Func<T, bool>>(block, predicate.Parameters);
            return lambda;
        }
    }
}
