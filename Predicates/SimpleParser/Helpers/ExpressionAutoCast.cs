// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IAmRaf.SimpleParser.Helpers
{
    /// <summary>
    /// Utilities to apply a numeric type conversion to a list of Expressions.
    /// This is useful for example to BinaryExpression instances needing the Left
    /// and Right operand must have the same type.
    /// This is needed because the Expression library does not accept/handle
    /// implicit conversions (which are done by the front-end compiler).
    /// </summary>
    internal static class ExpressionAutoCast
    {
        private enum IntPriority
        {
            None,
            TypeByte,
            TypeUInt16,
            TypeInt16,
            TypeUInt32,
            TypeInt32,
            TypeUInt64,
            TypeInt64,
        }

        private enum FloatPriority
        {
            None,
            TypeSingle,
            TypeDouble,
        }

        private enum DecimalPriority
        {
            None,
            TypeDecimal,
        }

        /// <summary>
        /// Add an Expression.Convert to the numeric expressions requiring the cast
        /// </summary>
        /// <param name="expressions">The list of expressions to evaluate</param>
        /// <returns>A new list of the same expressions where each one has been casted, if needed, to the correct type</returns>
        public static IList<Expression> ApplyNumericTypeConversion(params Expression[] expressions)
        {
            return ApplyNumericTypeConversion(null, null, null, expressions);
        }


        /// <summary>
        /// Add an Expression.Convert to the numeric expressions requiring the cast
        /// </summary>
        /// <param name="highestFloat">Type highest float type to impose as a minimum required cast</param>
        /// <param name="expressions">The list of expressions to evaluate</param>
        /// <returns>A new list of the same expressions where each one has been casted, if needed, to the correct type</returns>
        public static IList<Expression> ApplyNumericTypeConversion(Type highestFloat, params Expression[] expressions)
        {
            return ApplyNumericTypeConversion(null, highestFloat, null, expressions);
        }

        /// <summary>
        /// Add an Expression.Convert to the numeric expressions requiring the cast
        /// </summary>
        /// <param name="highestFloat">Type highest float type to impose as a minimum required cast</param>
        /// <param name="expressions">The list of expressions to evaluate</param>
        /// <returns>A new list of the same expressions where each one has been casted, if needed, to the correct type</returns>
        public static IList<Expression> ApplyNumericTypeConversion(Type highestFloat, IEnumerable<Expression> expressions)
        {
            return ApplyNumericTypeConversion(null, highestFloat, null, expressions.ToArray());
        }

        /// <summary>
        /// Add an Expression.Convert to the numeric expressions requiring the cast
        /// </summary>
        /// <param name="highestIntType">Type highest float int to impose as a minimum required cast</param>
        /// <param name="highestFloatType">Type highest float type to impose as a minimum required cast</param>
        /// <param name="highestDecimalType">Type highest decimal type to impose as a minimum required cast</param>
        /// <param name="expressions">The list of expressions to evaluate</param>
        /// <returns>A new list of the same expressions where each one has been casted, if needed, to the correct type</returns>
        public static IList<Expression> ApplyNumericTypeConversion(Type highestIntType,
            Type highestFloatType, Type highestDecimalType, params Expression[] expressions)
        {
            IntPriority highestInt = GetIntPriority(highestIntType);
            FloatPriority highestFloat = GetFloatPriority(highestFloatType);
            DecimalPriority highestDecimal = GetDecimalPriority(highestDecimalType);
            Type intType = highestIntType;
            Type floatType = highestFloatType;
            Type decimalType = highestDecimalType;
            bool allEquals = true;

            foreach (var expression in expressions)
            {
                IntPriority currentInt = IntPriority.None;
                FloatPriority currentFloat = FloatPriority.None;
                DecimalPriority currentDecimal = DecimalPriority.None;

                currentInt = GetIntPriority(expression.Type);
                if (currentInt == IntPriority.None)
                {
                    currentFloat = GetFloatPriority(expression.Type);
                    if (currentFloat == FloatPriority.None)
                    {
                        currentDecimal = GetDecimalPriority(expression.Type);
                        if (currentDecimal == DecimalPriority.None)
                        {
                            Debug.WriteLine("LevelType> at least one expression was not a number");
                            return expressions;
                        }
                    }
                }

                highestInt = Max(highestInt, currentInt);
                highestFloat = Max(highestFloat, currentFloat);
                highestDecimal = Max(highestDecimal, currentDecimal);
                if (highestInt == currentInt)
                {
                    if (intType != expression.Type)
                    {
                        allEquals = false;
                        intType = expression.Type;
                    }
                }

                if (highestFloat == currentFloat)
                {
                    if (floatType != expression.Type)
                    {
                        allEquals = false;
                        floatType = expression.Type;
                    }
                }

                if (highestDecimal == currentDecimal)
                {
                    if (decimalType != expression.Type)
                    {
                        allEquals = false;
                        decimalType = expression.Type;
                    }
                }
            }

            if (allEquals)
            {
                // no need to cast
                return expressions;
            }

            if (highestDecimal != DecimalPriority.None)
            {
                if (highestFloat != FloatPriority.None)
                {
                    // if there are decimals and floats or doubles ==> double
                    return CastTo(expressions, typeof(double));
                }

                return CastTo(expressions, typeof(decimal));
            }

            if (highestFloat == FloatPriority.None)
            {
                return CastTo(expressions, intType);
            }

            return CastTo(expressions, floatType);
        }

        /// <summary>
        /// Apply the cast to all the expressions, unless the type is already correct
        /// </summary>
        /// <param name="expressions">The input list of expressions</param>
        /// <param name="type">The type to cast to</param>
        /// <returns>The new list of expressions</returns>
        private static IList<Expression> CastTo(IEnumerable<Expression> expressions, Type type)
        {
            var result = new List<Expression>();
            foreach (var expression in expressions)
            {
                if (expression.Type == type)
                {
                    result.Add(expression);
                }
                else
                {
                    result.Add(Expression.Convert(expression, type));
                }
            }

            return result;
        }

        /// <summary>
        /// The Max operator for enumeration types
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="t1">An enum instance</param>
        /// <param name="t2">Another enum instance</param>
        /// <returns>The maximum (in terms of numeric value) instance</returns>
        private static T Max<T>(T t1, T t2) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            if (t1.ToInt64(null) >= t2.ToInt64(null))
            {
                return t1;
            }

            return t2;
        }

        /// <summary>
        /// Classify the type
        /// </summary>
        /// <param name="type">Type type to classify</param>
        /// <returns>The priority enumeration</returns>
        private static IntPriority GetIntPriority(Type type)
        {
            switch (type?.FullName)
            {
                case "System.Byte":
                    return IntPriority.TypeByte;

                case "System.UInt16":
                    return IntPriority.TypeUInt16;
                case "System.Int16":
                    return IntPriority.TypeInt16;

                case "System.UInt32":
                    return IntPriority.TypeUInt32;
                case "System.Int32":
                    return IntPriority.TypeInt32;

                case "System.UInt64":
                    return IntPriority.TypeUInt64;
                case "System.Int64":
                    return IntPriority.TypeInt64;

                default:
                    return IntPriority.None;
            }

        }

        /// <summary>
        /// Classify the type
        /// </summary>
        /// <param name="type">Type type to classify</param>
        /// <returns>The priority enumeration</returns>
        private static FloatPriority GetFloatPriority(Type type)
        {
            switch (type?.FullName)
            {
                case "System.Single":
                    return FloatPriority.TypeSingle;

                case "System.Double":
                    return FloatPriority.TypeDouble;

                default:
                    return FloatPriority.None;
            }
        }

        /// <summary>
        /// Classify the type
        /// </summary>
        /// <param name="type">Type type to classify</param>
        /// <returns>The priority enumeration</returns>
        private static DecimalPriority GetDecimalPriority(Type type)
        {
            switch (type?.FullName)
            {
                case "System.Decimal":
                    return DecimalPriority.TypeDecimal;

                default:
                    return DecimalPriority.None;
            }
        }
    }
}
