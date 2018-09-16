// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAmRaf.SimpleParser.AbstractTree
{
    /// <summary>
    /// A node representing an operator
    /// </summary>
    internal class NodeItemOperator : NodeItem
    {
        public NodeItemOperator(NodeItem parent, string name)
            : base(parent, name)
        {
        }
    }
}
