// Copyright (c) 2017, Raffaele Rialdi
//
using IAmRaf.SimpleParser.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IAmRaf.SimpleParser.Expressions
{
    /// <summary>
    /// This is an Operation representing a call to a function
    /// taking a single argument
    /// </summary>
    internal class OperationCallArgument1 : OperationCall
    {
        public OperationCallArgument1(ExpressionType expressionType, MethodData methodData)
            : base(expressionType, methodData)
        {
            Function = argument => Expression.Call(null, MethodData.MethodInfo, FixParameterCast(argument, 0));
        }


        public Func<Expression, Expression> Function { get; }
    }
}
