// Copyright (c) 2017, Raffaele Rialdi
//
using IAmRaf.SimpleParser.AbstractTree;
using IAmRaf.SimpleParser.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IAmRaf.SimpleParser.Expressions
{
    /// <summary>
    /// This class is a factory for OperationBase derived classes
    /// The goal is to transform a NodeItem into an OperationBase derived instance
    /// holding the basic information to create an Expression and the Expression factory
    /// that will be called as soon as all the mandatory parameters are available.
    /// The OperationBase set of classes represent the categories of allowed operations:
    /// - Unary operations such as "negate"
    /// - Binary operations such as adding, multiplying, etc.
    /// - Call operations that represent an external function call. They are then furtherly
    ///   categorized in a hierarchy that depends on the input parameters of the call.
    /// </summary>
    internal static class OperationBuilder
    {
        public static OperationBase GetOperation(NodeItem item)
        {
            var name = item.Name;
            var unary = GetUnary(name, item.Kind);
            if (unary.HasValue)
            {
                return new OperationUnary(unary.Value.expressionType, unary.Value.func);
            }

            var binary = GetBinary(item.Name);
            if (binary.HasValue)
            {
                return new OperationBinary(binary.Value.expressionType, binary.Value.func);
            }

            var call = GetCall(item.Name);
            if (call.HasValue)
            {
                var callValue = call.Value;
                var expressionType = callValue.expressionType;
                var methodData = callValue.methodData;
                if (callValue.methodData.IsOperator)
                {
                    return new OperationCallOperator2(expressionType, methodData);
                }

                if (callValue.methodData.IsParams)
                {
                    return new OperationCallParamsArguments(expressionType, methodData);
                }

                if (callValue.methodData.ParameterInfos.Length == 1)
                {
                    return new OperationCallArgument1(expressionType, methodData);
                }

                return new OperationCallMultipleArguments(expressionType, methodData);
            }

            throw new Exception($"Unsupported operator or method: {item.Name}");
        }

        private static (ExpressionType expressionType, Func<Expression, Expression> func)? GetUnary(string name, NodeKind nodeKind)
        {
            switch (name)
            {
                case "+":
                    if (nodeKind == NodeKind.OpAdditive) return null;
                    return (ExpressionType.UnaryPlus, Expression.UnaryPlus);

                case "-":
                    if (nodeKind == NodeKind.OpAdditive) return null;
                    return (ExpressionType.Negate, Expression.Negate);

                case "!":
                    return (ExpressionType.Not, Expression.Not);

                case "~":
                    return (ExpressionType.OnesComplement, Expression.OnesComplement);

                default:
                    return null;
            }
        }

        private static (ExpressionType expressionType, Func<Expression, Expression, Expression> func)? GetBinary(string name)
        {
            switch (name)
            {
                case "+":
                    return (ExpressionType.Add, Expression.Add);

                case "-":
                    return (ExpressionType.Subtract, Expression.Subtract);

                case "*":
                    return (ExpressionType.Multiply, Expression.Multiply);

                case "/":
                    return (ExpressionType.Divide, Expression.Divide);

                case "%":
                    return (ExpressionType.Modulo, Expression.Modulo);

                case "<<":
                    return (ExpressionType.LeftShift, Expression.LeftShift);

                case ">>":
                    return (ExpressionType.RightShift, Expression.RightShift);

                case ">":
                    return (ExpressionType.GreaterThan, Expression.GreaterThan);

                case ">=":
                    return (ExpressionType.GreaterThanOrEqual, Expression.GreaterThanOrEqual);

                case "<":
                    return (ExpressionType.LessThan, Expression.LessThan);

                case "<=":
                    return (ExpressionType.LessThanOrEqual, Expression.LessThanOrEqual);

                case "==":
                    return (ExpressionType.Equal, Expression.Equal);

                case "!=":
                    return (ExpressionType.NotEqual, Expression.NotEqual);

                case "&":
                    return (ExpressionType.And, Expression.And);

                case "|":
                    return (ExpressionType.Or, Expression.Or);

                case "&&":
                    return (ExpressionType.AndAlso, Expression.AndAlso);

                case "||":
                    return (ExpressionType.OrElse, Expression.OrElse);

                // this operator has been used for Math.Pow instead
                //case "^":
                //    return (ExpressionType.ExclusiveOr, Expression.ExclusiveOr);

                default:
                    return null;
            }
        }

        private static (ExpressionType expressionType, MethodData methodData)? GetCall(string name)
        {
            bool isOperator = false;
            if (name == "^")
            {
                // Math.Pow is a special case because we want to use the operator "^" which is instead used by C# as the XOR operator
                // We want to use it as an alias for the "POW" function defined in the FunctionsHelper class
                // In this example: 2*x^2 + 3*x + 1
                // the called function is POW(x,2)
                name = "POW";
                isOperator = true;
            }

            var methodData = FunctionsHelper.Get(name);
            if (methodData != null)
            {
                methodData.IsOperator = isOperator;
                return (ExpressionType.Call, methodData);
            }

            return null;
        }

    }
}
