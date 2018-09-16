// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace IAmRaf.SimpleParser.Expressions
{
    internal class OperationBinary : OperationBase
    {
        public OperationBinary(ExpressionType expressionType, Func<Expression, Expression, Expression> function)
        {
            this.ExpressionType = expressionType;
            this.Function = function;
        }

        /// <summary>
        /// The factory taking two expressions and returning the BinaryExpression
        /// </summary>
        public Func<Expression, Expression, Expression> Function { get; set; }
        public Expression Left { get; set; }
        public Expression Right { get; set; }
    }
}
