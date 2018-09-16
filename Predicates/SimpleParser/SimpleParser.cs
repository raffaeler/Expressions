// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IAmRaf.SimpleParser.Helpers;
using IAmRaf.SimpleParser.Expressions;
using IAmRaf.SimpleParser.AbstractTree;

/*
Usage examples:
var e1 = SimpleParser.Parse("x >=1", new Parameter(typeof(int), "x"));
var e2 = SimpleParser.Parse("x+4<=2", new Parameter(typeof(int), "x"));
var e3 = SimpleParser.Parse("x + 5 > 9", new Parameter(typeof(int), "x"));
var e4 = SimpleParser.Parse("2+(x + 5)* 3 >1", new Parameter(typeof(int), "x"));
var e5 = SimpleParser.Parse("x == \"raf\"", new Parameter(typeof(string), "x"));
*/

namespace IAmRaf.SimpleParser
{
    /// <summary>
    /// This is the entry point to parse a string and convert into an Expression
    /// </summary>
    public sealed class SimpleParser
    {
        private Parameter[] _parameters;

        private SimpleParser()
        {
            //_nodes = new List<NodeItem>();
        }

        public static ParseResult Parse(string expression, Parameter parameter)
        {
            return Parse(expression, null, new Parameter[] { parameter });
        }

        public static ParseResult Parse(string expression, params Parameter[] parameters)
        {
            var parser = new SimpleParser();
            return parser.ParseInternal(expression, null, parameters);
        }

        public static ParseResult Parse(string expression, Type forcedOutputType, Parameter parameter)
        {
            return Parse(expression, forcedOutputType, new Parameter[] { parameter });
        }

        public static ParseResult Parse(string expression, Type forcedOutputType, params Parameter[] parameters)
        {
            var parser = new SimpleParser();
            return parser.ParseInternal(expression, forcedOutputType, parameters);
        }

        private ParseResult ParseInternal(string text, Type forcedOutputType, params Parameter[] parameters)
        {
            _parameters = parameters;
            var tokenizer = new Tokenizer();
            var nodes = tokenizer.Tokenize(text);
            tokenizer.AggregateOperators(nodes);
            tokenizer.Categorize(nodes, _parameters);

            var expressionBuilder = new ExpressionBuilder(parameters, forcedOutputType);
            var expression = expressionBuilder.BuildExpression(nodes);

            var lambda = Expression.Lambda(expression, expressionBuilder.Parameters);
            return new ParseResult(lambda);
        }
    }
}
