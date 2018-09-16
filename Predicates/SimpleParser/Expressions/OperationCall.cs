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
    /// This is the base class representing a call to a function
    /// </summary>
    internal abstract class OperationCall : OperationBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expressionType"></param>
        /// <param name="methodInfo"></param>
        public OperationCall(ExpressionType expressionType, MethodData methodData)
        {
            this.ExpressionType = expressionType;
            this.MethodData = methodData;
        }

        
        public MethodData MethodData { get; private set; }

        protected Expression FixParameterCast(Expression expression, int parameterIndex)
        {
            Type type;
            if (MethodData.ParameterInfos.Length <= parameterIndex || IsParams(MethodData.ParameterInfos[parameterIndex]))
            {
                // params is always applied on the last
                var parameter = MethodData.ParameterInfos.Last();
                // type is an array but we need the element type
                type = parameter.ParameterType.GetElementType();
            }
            else
            {
                var parameter = MethodData.ParameterInfos[parameterIndex];
                type = parameter.ParameterType;
            }

            if (type == expression.Type)
            {
                return expression;  // no cast
            }

            return Expression.Convert(expression, type);
        }

        private static bool IsParams(ParameterInfo parameterInfo)
        {
            return parameterInfo.GetCustomAttributes(typeof(ParamArrayAttribute), false).Any();
        }
    }
}
