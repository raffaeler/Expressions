// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IAmRaf.SimpleParser.Expressions
{
    /// <summary>
    /// This is an Operation representing a call to a function
    /// taking two arguments (specialization for Binary Expressions)
    /// Important Note: variable number of parameters are supported only if the params is the only parameter
    /// If anyone wants to support other parameters before the params, the change is in this class.
    /// The change should create a list of Expressions where the last one is the NewArrayExpression 
    /// representing all the params arguments
    /// </summary>
    internal class OperationCallParamsArguments : OperationCall
    {
        public OperationCallParamsArguments(ExpressionType expressionType, MethodData methodData)
            : base(expressionType, methodData)
        {
            Function = arguments =>
            {
                var casted = arguments.Select(FixParameterCast);
                return Expression.Call(null, MethodData.MethodInfo,
                    Expression.NewArrayInit(casted.First().Type, casted));
            };
        }

        public Func<IList<Expression>, Expression> Function { get; }
    }
}
