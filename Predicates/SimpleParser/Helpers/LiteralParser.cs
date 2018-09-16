// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IAmRaf.SimpleParser.Helpers
{
    /// <summary>
    /// Provides a method to parse a literal:
    /// - numbers are parsed appropriately (double has the precedence over decimal)
    /// - string must start/end with a double quote
    /// - every other literal is not accepted and throws
    /// </summary>
    internal static class LiteralParser
    {
        public static (object value, Type type) ParseLiteral(string text)
        {
            object value = null;
            Type type = null;
            if (text.Contains('.'))
            {
                double num;
                if (double.TryParse(text, out num))
                {
                    value = num;
                    type = typeof(double);
                }
            }

            if (value == null)
            {
                int num;
                if (int.TryParse(text, out num))
                {
                    value = num;
                    type = typeof(int);
                }
            }

            if (value == null)
            {
                if (text.StartsWith("\"") && text.EndsWith("\""))
                {
                    value = text.Trim('\"');
                    type = typeof(string);
                }
            }

            if (value == null || type == null)
                throw new Exception("Invalid literal: " + text);

            return (value, type);
        }
    }
}
