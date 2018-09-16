// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IAmRaf.SimpleParser.AbstractTree
{
    /// <summary>
    /// Transfrom the textual expression in tokens
    /// </summary>
    internal class Tokenizer
    {
        private StringBuilder _builder;
        private NodeItem _current;

        /// <summary>
        /// This method tokenize the string expression.
        /// It is the very first step to do to parse text and produces
        /// a tree structure that will be further elaborated
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IList<NodeItem> Tokenize(string expression)
        {
            _builder = new StringBuilder();
            var root = new NodeItem(null, ""); // root
            _current = root;
            bool _lastParenthesisWasNotDown = false;

            Down();
            foreach (var ch in expression)
            {
                switch (ch)
                {
                    case '(':
                        if (BuilderIsEmpty())
                        {
                            Down();
                        }
                        else
                        {
                            //Commit();
                            //_builder.Append(ch);
                            _lastParenthesisWasNotDown = true;
                            CommitDownFunction();
                        }
                        break;

                    case ')':
                        if (_lastParenthesisWasNotDown)
                        {
                            Commit();
                            //_builder.Append(ch);
                            //Commit();
                            Up();
                            _lastParenthesisWasNotDown = false;
                        }
                        else
                        {
                            Commit();
                            Up();
                        }
                        break;

                    case '^':
                    case '%':
                    case '!':
                    case '~':
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '>':
                    case '<':
                    case '=':
                    case '&':
                    case '|':
                        Commit();
                        CommitSpecial(ch);
                        break;

                    case ',':
                        Commit();
                        CommitSpecial(ch);
                        break;

                    case ' ':
                        Commit();
                        _builder.Append(ch);
                        Commit();
                        break;

                    default:
                        _builder.Append(ch);
                        break;
                }
            }

            Commit();
            Up();
            root.RemoveEmptyChildren();

            var str = root.ToString();
            Debug.WriteLine(string.Format("Before: {0}\r\nAfter:  {1}", expression, str));

            if (_current.Parent != null)
            {
                throw new Exception("Parenthesis error");
            }

            return _current.NodeItems;
        }

        /// <summary>
        /// The second step is to aggregate the operators like ">" "=" in ">="
        /// </summary>
        /// <param name="nodeItems"></param>
        public void AggregateOperators(IList<NodeItem> nodeItems)
        {
            var todelete = new List<NodeItem>();
            NodeItem previous = null;
            foreach (var node in nodeItems)
            {
                if (previous == null)
                {
                    previous = node;
                    continue;
                }

                AggregateOperators(node.NodeItems);


                if (previous is NodeItemOperator && node is NodeItemOperator)
                {
                    previous.AppendName(node.Name);
                    todelete.Add(node);
                }

                previous = node;
            }

            foreach (var item in todelete)
            {
                nodeItems.Remove(item);
            }
        }

        /// <summary>
        /// The third step is to assign a category to each node of the tree
        /// </summary>
        /// <param name="nodeItems"></param>
        /// <param name="inputParameters"></param>
        public void Categorize(IList<NodeItem> nodeItems, Parameter[] inputParameters)
        {
            NodeItem previous = null;
            foreach (var node in nodeItems)
            {
                NodeKind kind;
                switch (node.Name)
                {
                    case "+":   // unary, additive
                    case "-":   // unary, additive
                    case "!":
                    case "~":
                        if (previous is NodeItemOperator)
                        {
                            kind = NodeKind.OpUnary;
                        }
                        else
                        {
                            kind = NodeKind.OpAdditive;
                        }
                        break;

                    case "*":
                    case "/":
                    case "%":
                        kind = NodeKind.OpMultiplicative;
                        break;

                    case "<<":
                    case ">>":
                        kind = NodeKind.OpShift;
                        break;

                    case ">":
                    case "<":
                    case "<=":
                    case ">=":
                        kind = NodeKind.OpRelational;
                        break;

                    case "==":
                    case "!=":
                        kind = NodeKind.OpEquality;
                        break;

                    case "&":
                        kind = NodeKind.OpLogicalAnd;
                        break;

                    //case "^":
                    //    kind = NodeKind.OpLogicalXor;
                    //    break;

                    case "|":
                        kind = NodeKind.OpLogicalOr;
                        break;


                    case "&&":
                        kind = NodeKind.OpConditionalAnd;
                        break;

                    case "||":
                        kind = NodeKind.OpConditionalOr;
                        break;

                    case "":
                        kind = NodeKind.SubExpression;
                        break;

                    case "^":
                        kind = NodeKind.OpCall;
                        break;

                    // new
                    case ",":
                        kind = NodeKind.Separator;
                        break;

                    default:
                        {
                            if (inputParameters.Select(n => n.Name).Contains(node.Name))
                            {
                                kind = NodeKind.Parameter;
                            }
                            else if (node is NodeFunction)
                            {
                                kind = NodeKind.Function;
                            }
                            else
                            {
                                kind = NodeKind.Literal;
                            }
                        }
                        break;
                }

                node.SetKind(kind);
                previous = node;
                Categorize(node.NodeItems, inputParameters);
            }
        }

        private bool BuilderIsEmpty()
        {
            var text = _builder.ToString().Trim();
            if (text == string.Empty)
                return true;

            return false;
        }

        private void Commit()
        {
            var text = _builder.ToString().Trim();
            if (text == string.Empty)
            {
                return;
            }

            _builder.Clear();
            _current = _current.AddSibling(text);
        }

        private void CommitSpecial(char symbol)
        {
            var text = symbol.ToString();
            _current = _current.AddSiblingOperator(text);
        }

        private void Down()
        {
            if (_current.Parent != null)
                _current = _current.AddSibling("");
            _current = _current.AddChild("");
        }

        // Used only for functions so that parameters are the children
        private void CommitDownFunction()
        {
            var text = _builder.ToString().Trim();
            _builder.Clear();
            _current = _current.AddSiblingFunction(text);
            _current = _current.AddChild("");
        }

        private void Up()
        {
            _current = _current.Parent;
        }
    }
}
