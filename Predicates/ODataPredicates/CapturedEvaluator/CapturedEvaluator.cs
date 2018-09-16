// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

// Inspired by the following MSDN Sample 
// https://msdn.microsoft.com/en-us/library/bb546158.aspx

namespace ODataPredicates.CapturedEvaluator
{
    public class CapturedEvaluator : ExpressionVisitor
    {
        private HashSet<Expression> _results;
        private bool _analyze;
        private bool _evaluateSubTree;

        private CapturedEvaluator()
        {
            _results = new HashSet<Expression>();
        }

        public static Expression Eval(Expression expression)
        {
            var visitor = new CapturedEvaluator();
            visitor._analyze = true;
            visitor.Visit(expression);  // step1: analyze

            visitor._analyze = false;
            var evaluated = visitor.Visit(expression);  // step2: replace
            return evaluated;
        }

        public override Expression Visit(Expression expression)
        {
            if (_analyze)
            {
                VisitAnalyze(expression);
                return expression;
            }

            return VisitReplace(expression);
        }

        private Expression VisitAnalyze(Expression node)
        {
            if (node == null) return null;
            var preserve = _evaluateSubTree;
            _evaluateSubTree = true;
            base.Visit(node);
            if (_evaluateSubTree)
            {
                if (node.NodeType != ExpressionType.Parameter)
                {
                    _results.Add(node);
                }
                else
                {
                    if (node.NodeType != ExpressionType.Constant)
                    {
                        _evaluateSubTree = false;
                    }
                }
            }
            _evaluateSubTree &= preserve;
            return node;
        }

        private Expression VisitReplace(Expression node)
        {
            if (node == null) return null;
            if (_results.Contains(node))
            {
                try
                {
                    var lambda = Expression.Lambda(node);
                    var deleg = lambda.Compile();
                    var value = deleg.DynamicInvoke(null);
                    var replaced = Expression.Constant(value, node.Type);
                    return replaced;
                }
                catch (Exception err)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "VisitReplace error: " + err.ToString());
                    return node;
                }
            }

            return base.Visit(node);
        }
    }
}
