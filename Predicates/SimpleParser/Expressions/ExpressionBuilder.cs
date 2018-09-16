// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using IAmRaf.SimpleParser.AbstractTree;
using IAmRaf.SimpleParser.Helpers;

namespace IAmRaf.SimpleParser.Expressions
{
    /// <summary>
    /// Encapsulate the logic to transform a tree of NodeItems into
    /// a Linq Expression
    /// </summary>
    internal class ExpressionBuilder
    {
        private Parameter[] _parameters;
        private Type _forcedOutputType;

        /// <summary>
        /// This class transforms the NodeItems into a Linq Expression
        /// </summary>
        /// <param name="parameters">The input parameters declared from the user</param>
        /// <param name="forcedOutputType">The type the user want to obtain as output. It can be null</param>
        public ExpressionBuilder(Parameter[] parameters, Type forcedOutputType)
        {
            _parameters = parameters;
            _forcedOutputType = forcedOutputType;
        }

        /// <summary>
        /// A list of ParameterExpression elements representing the input parameters of the parsed expression
        /// </summary>
        public IList<ParameterExpression> Parameters { get; private set; }

        /// <summary>
        /// The method called to transform a list of nodes into a Linq Expression
        /// </summary>
        /// <param name="nodeItems"></param>
        /// <returns></returns>
        public Expression BuildExpression(IList<NodeItem> nodeItems)
        {
            Parameters = new List<ParameterExpression>();
            return BuildExpressionInternal(nodeItems);
        }

        private Expression BuildExpressionInternal(IList<NodeItem> nodeItems)
        {
            var min = nodeItems.Max(n => n.Kind);

            NodeItem lowest = nodeItems.FirstOrDefault(n => n.Kind == min);
            if (lowest.Kind == NodeKind.SubExpression)
            {
                nodeItems = lowest.NodeItems;
                min = nodeItems.Max(n => n.Kind);
                lowest = nodeItems.SingleOrDefault(n => n.Kind == min);
            }

            var index = nodeItems.IndexOf(lowest);
            var previousSiblings = nodeItems.Take(index).ToList();
            var nextSiblings = nodeItems.Skip(index + 1).ToList();

            var result = BuildExpressionFrom(lowest, previousSiblings, nextSiblings);

            if (_forcedOutputType != null && _forcedOutputType != result.Type)
            {
                result = Expression.Convert(result, _forcedOutputType);
            }

            return result;
        }

        private Expression BuildExpressionFrom(NodeItem nodeItem, IList<NodeItem> previous, IList<NodeItem> next)
        {
            if (nodeItem.Kind == NodeKind.Literal)
            {
                var parsed = LiteralParser.ParseLiteral(nodeItem.Name);

                return Expression.Constant(parsed.value, parsed.type);
            }

            if (nodeItem.Kind == NodeKind.Parameter)
            {
                var par = _parameters.FirstOrDefault(p => p.Name == nodeItem.Name);
                if (par == null)
                {
                    throw new Exception("An undeclared parameter was found " + nodeItem.Name);
                }

                var parameterExpression = Parameters
                    .FirstOrDefault(p => p.Name == nodeItem.Name);

                if (parameterExpression == null)
                {
                    parameterExpression = Expression.Parameter(par.Type, nodeItem.Name);
                    Parameters.Add(parameterExpression);
                }

                return parameterExpression;
            }

            var operation = OperationBuilder.GetOperation(nodeItem);

            switch (operation)
            {
                case OperationUnary operationUnary:
                    {
                        if (previous.Count > 0 || next.Count == 0)
                        {
                            throw new Exception("An unary expression was expected");
                        }

                        var argument = BuildExpressionInternal(next);
                        return operationUnary.Function(argument);
                    }

                case OperationBinary operationBinary:
                    {
                        if (previous.Count == 0 || next.Count == 0)
                        {
                            throw new Exception("A binary expression was expected");
                        }

                        var left = BuildExpressionInternal(previous);
                        var right = BuildExpressionInternal(next);
                        var casted = ExpressionAutoCast.ApplyNumericTypeConversion(left, right);

                        return operationBinary.Function(casted[0], casted[1]);
                    }

                case OperationCallOperator2 operationCallOperator2:
                    {
                        // an operator has left and right operands. For example: x^2  ==> POW(x, 2.0)
                        if (previous.Count == 0 || next.Count == 0)
                        {
                            throw new Exception("expected one left an done right operands");
                        }

                        var left = BuildExpressionInternal(previous);
                        var right = BuildExpressionInternal(next);
                        return operationCallOperator2.Function(left, right);
                    }

                case OperationCallArgument1 operationCallArgument1:
                    {
                        // a call with a single operand
                        var arguments = GetCallArguments(nodeItem.NodeItems);
                        if (arguments.Count != 1)
                        {
                            throw new InvalidOperationException($"The call {nodeItem.Name} expected a single argument");
                        }

                        return operationCallArgument1.Function(arguments.Single());
                    }

                case OperationCallMultipleArguments operationCallMultipleArguments:
                    {
                        // a call with a single operand
                        var arguments = GetCallArguments(nodeItem.NodeItems);
                        return operationCallMultipleArguments.Function(arguments);
                    }

                case OperationCallParamsArguments operationCallParamsArguments:
                    {
                        // has operands passed as T[]
                        var arguments = GetCallArguments(nodeItem.NodeItems);
                        return operationCallParamsArguments.Function(arguments);
                    }

                default:
                    throw new NotImplementedException($"The operator {nodeItem} is not implemented");
            }
        }

        /// <summary>
        /// In the textual expression, the arguments are comma-separated
        /// The "," symbol has already been converted to a NodeItem of NodeKind.Separator.
        /// But every operand can be an expression as well.
        /// The goal of this method is to take the groups of operands, build an expression for
        /// each of them and return a List of Expressions for all these groups
        /// The expression "SUM(x*2, 1") has two groups: "x*2" and "1"
        /// This method will produce two expressions representing the two groups
        /// </summary>
        /// <param name="nodeItems"></param>
        /// <returns></returns>
        private List<Expression> GetCallArguments(IList<NodeItem> nodeItems)
        {
            // 1. group the elements by the separator
            var groups = new List<List<NodeItem>>();
            var subExprItems = new List<NodeItem>();
            foreach (var item in nodeItems)
            {
                if (item.Kind != NodeKind.Separator)
                {
                    subExprItems.Add(item);
                }
                else
                {
                    groups.Add(subExprItems);
                    subExprItems = new List<NodeItem>();
                }
            }

            if (subExprItems.Count > 0)
            {
                groups.Add(subExprItems);
            }

            // 2. transform the subexpressions in "Expression"
            var arguments = new List<Expression>();
            foreach (var group in groups)
            {
                var exp = BuildExpressionInternal(group);
                arguments.Add(exp);
            }

            return arguments;
        }
    }
}
