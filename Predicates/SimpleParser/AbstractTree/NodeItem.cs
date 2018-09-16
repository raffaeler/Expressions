// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAmRaf.SimpleParser.AbstractTree
{
    /// <summary>
    /// A generic node of the tree representing the parsed expression
    /// </summary>
    [DebuggerDisplay("{Name} {Kind} [{NodeItems.Count}]")]
    internal class NodeItem
    {
        public NodeItem(NodeItem parent, string name)
        {
            NodeItems = new List<NodeItem>();
            this.Parent = parent;
            this.Name = name;
        }

        /// <summary>
        /// The name of the node
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The kind/category of the node
        /// </summary>
        public NodeKind Kind { get; protected set; }

        /// <summary>
        /// The list of children nodes
        /// </summary>
        public IList<NodeItem> NodeItems { get; private set; }

        /// <summary>
        /// The parent node
        /// </summary>
        public NodeItem Parent { get; protected set; }

        /// <summary>
        /// Append a name of the current node name
        /// </summary>
        /// <param name="name">The name to append</param>
        public void AppendName(string name)
        {
            this.Name += name;
        }

        /// <summary>
        /// Set the kind of the node
        /// </summary>
        /// <param name="kind"></param>
        public void SetKind(NodeKind kind)
        {
            if (Kind != NodeKind.Unknown)
                throw new Exception("Cannot change NodeKind after it has been already set once");

            this.Kind = kind;
        }

        /// <summary>
        /// Add a sibling node to the current one
        /// </summary>
        /// <param name="name">The name of the newly created node</param>
        /// <returns>The new node</returns>
        public NodeItem AddSibling(string name)
        {
            var newNode = new NodeItem(this.Parent, name);
            newNode.Kind = NodeKind.Unknown;
            Parent.NodeItems.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// Add a sibling node of type operator to the current one
        /// </summary>
        /// <param name="name">The name of the newly created node</param>
        /// <returns>The new node</returns>
        public NodeItem AddSiblingOperator(string name)
        {
            var newNode = new NodeItemOperator(this.Parent, name);
            newNode.Kind = NodeKind.Unknown;
            Parent.NodeItems.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// Add a sibling node of type function to the current one
        /// </summary>
        /// <param name="name">The name of the newly created node</param>
        /// <returns>The new node</returns>
        public NodeItem AddSiblingFunction(string name)
        {
            var newNode = new NodeFunction(this.Parent, name);
            newNode.Kind = NodeKind.Unknown;
            Parent.NodeItems.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// Add a child node to the current one
        /// </summary>
        /// <param name="name">The name of the newly created node</param>
        /// <returns>The new node</returns>
        public NodeItem AddChild(string name)
        {
            var newNode = new NodeItem(this, name);
            this.NodeItems.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// Remove all the empty nodes from the children list
        /// </summary>
        public void RemoveEmptyChildren()
        {
            var todelete = new List<NodeItem>();
            foreach (var item in NodeItems)
            {
                item.RemoveEmptyChildren();

                if (string.IsNullOrEmpty(item.Name) && item.NodeItems.Count == 0)
                {
                    todelete.Add(item);
                }
            }

            foreach (var del in todelete)
            {
                NodeItems.Remove(del);
            }
        }

        /// <summary>
        /// Build a textual representation of this subtree starting from the current node
        /// </summary>
        /// <param name="sb"></param>
        private void Accumulate(StringBuilder sb)
        {
            sb.Append(this.Name);
            if (NodeItems.Count > 0)
            {
                sb.Append('(');
                {
                    foreach (var child in NodeItems)
                    {
                        child.Accumulate(sb);
                    }
                }

                sb.Append(')');
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            Accumulate(sb);
            return sb.ToString();
        }
    }
}
