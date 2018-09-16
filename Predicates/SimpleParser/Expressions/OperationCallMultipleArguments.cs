// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IAmRaf.SimpleParser.Expressions
{
    /// <summary>
    /// This is an Operation representing a call to a function
    /// taking multiple arguments
    /// </summary>
    internal class OperationCallMultipleArguments : OperationCall
    {
        public OperationCallMultipleArguments(ExpressionType expressionType, MethodData methodData)
            : base(expressionType, methodData)
        {
            Function = arguments => Expression.Call(null, MethodData.MethodInfo, arguments.Select(FixParameterCast));
        }

        public Func<IList<Expression>, Expression> Function { get; }
    }
}
