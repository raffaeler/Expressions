// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IAmRaf.SimpleParser
{
    /// <summary>
    /// The class representing the result of the parsing
    /// </summary>
    public class ParseResult
    {
        internal ParseResult(LambdaExpression expression)
        {
            this.Expression = expression;
        }

        /// <summary>
        /// The list of parameters
        /// </summary>
        public IList<ParameterExpression> Parameters
        {
            get
            {
                if (Expression == null)
                {
                    return null;
                }

                return Expression.Parameters;
            }
        }

        /// <summary>
        /// The lambda representing the parsed expression
        /// </summary>
        public LambdaExpression Expression { get; private set; }

        /// <summary>
        /// Compile the expression and returns a delegate
        /// </summary>
        /// <returns></returns>
        public Delegate Compile()
        {
            return Expression.Compile();
        }


        /// <summary>
        /// Compile the expression and returns a typed delegate
        /// </summary>
        /// <returns></returns>
        public T Compile<T>()
        {
            return (T)(object)Expression.Compile();
        }
    }
}
