// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace IAmRaf.SimpleParser.Expressions
{
    /// <summary>
    /// This is an Operation representing a call to a function
    /// taking a single argument
    /// </summary>
    internal class OperationUnary : OperationBase
    {
        public OperationUnary(ExpressionType expressionType, Func<Expression, Expression> function)
        {
            this.ExpressionType = expressionType;
            this.Function = function;
        }

        /// <summary>
        /// The factory taking one expression and returning the UnaryExpression
        /// </summary>
        public Func<Expression, Expression> Function { get; set; }
    }
}
