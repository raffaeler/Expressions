// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace IAmRaf.SimpleParser.Expressions
{
    /// <summary>
    /// A class holding information about a call to a method
    /// These are the basic info to build a method call expression
    /// </summary>
    internal class MethodData
    {
        public MethodData(string name, MethodInfo methodInfo, ParameterInfo[] parameterInfos, bool isParams)
        {
            this.Name = name;
            this.MethodInfo = methodInfo;
            this.ParameterInfos = parameterInfos;
            this.IsParams = isParams;
        }

        public string Name { get; }
        public MethodInfo MethodInfo { get; }
        public bool IsOperator { get; internal set; }
        public ParameterInfo[] ParameterInfos { get; }
        public bool IsParams { get; }
    }
}
