// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using IAmRaf.SimpleParser.Expressions;

namespace IAmRaf.SimpleParser
{
    /// <summary>
    /// This helper provides the functions that recognized from the parser
    /// The syntax of a function should be: "FUNC(arg1, arg2, arg3, ...)"
    /// where FUNC is the name of the function and the "arg" are the arguments.
    /// </summary>
    public static class FunctionsHelper
    {
        private static MethodInfo[] _funcs;
        private static readonly IDictionary<string, MethodInfo> Functions = new Dictionary<string, MethodInfo>();

        static FunctionsHelper()
        {
            _funcs = typeof(FunctionsHelper).GetMethods(BindingFlags.Public | BindingFlags.Static);
            Functions = new Dictionary<string, MethodInfo>();
            AddFunc("POW");
            AddFunc("SUM");
            AddFunc("AVG");
        }

        /// <summary>
        /// The list of the supported functions
        /// </summary>
        public static IEnumerable<string> SupportedFunctions
        {
            get { return Functions.Keys; }
        }

        /// <summary>
        /// Given the function name, it returns the MethodInfo 
        /// </summary>
        /// <param name="name">The function name</param>
        /// <returns>The MethodInfo for the function</returns>
        internal static MethodData Get(string name)
        {
            Functions.TryGetValue(name, out MethodInfo methodInfo);
            var parameters = methodInfo.GetParameters();
            bool isParams = parameters.Length != 0 ? IsParams(parameters.Last()) : false;
            return new MethodData(name, methodInfo, parameters, isParams);
        }

        private static void AddFunc(string name, string localName = null)
        {
            if (localName == null)
            {
                localName = name;
            }

            Functions[name] = GetFunc(localName);
        }

        private static MethodInfo GetFunc(string name)
        {
            return _funcs.Single(x => x.Name == name);
        }

        private static bool IsParams(ParameterInfo parameterInfo)
        {
            return parameterInfo.GetCustomAttributes(typeof(ParamArrayAttribute), false).Any();
        }

        public static double POW(double x, double y) => Math.Pow(x, y);
        public static double SUM(params double[] items) => items.Sum();
        public static double AVG(params double[] items) => items.Average();
    }
}
