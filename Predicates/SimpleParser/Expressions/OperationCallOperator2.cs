// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IAmRaf.SimpleParser.Expressions
{
    /// <summary>
    /// This is an Operation representing a call to a function
    /// taking two arguments (specialization for operators having left and right arguments)
    /// </summary>
    internal class OperationCallOperator2 : OperationCall
    {
        public OperationCallOperator2(ExpressionType expressionType, MethodData methodData)
            : base(expressionType, methodData)
        {
            // the binary operator has left and right arguments as opposed to
            // function calls that have parameters inside the parenthesis
            Function = (left, right) => Expression.Call(null, MethodData.MethodInfo, FixParameterCast(left, 0), FixParameterCast(right, 1));
        }

        public Func<Expression, Expression, Expression> Function { get; }
    }
}
