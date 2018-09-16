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
    internal class OperationBase
    {
        public ExpressionType ExpressionType { get; protected set; }
    }
}
