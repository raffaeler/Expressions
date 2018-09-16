// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Op categories: https://msdn.microsoft.com/en-us/library/aa691323(v=vs.71).aspx

namespace IAmRaf.SimpleParser.AbstractTree
{
    /// <summary>
    /// The kind/category of a node
    /// This enumeration also represent the order of precedence of the operators
    /// </summary>
    internal enum NodeKind
    {
        Unknown,
        Empty,
        SubExpression,
        Literal,
        Parameter,
        Function, // like SUM(, AVG(, MAX(, MIN(, etc.
        Separator,  // the comma "," used in Functions

        // operators in order of precedence (higher to lower)
        OpMax,              // just a delimiter to get the max priority number
        OpPrimary,          // x.y  f(x)  a[x]  x++  x--  new typeof  checked  unchecked
        OpCall,             // ^
        OpUnary,            // +  -  !  ~  ++x  --x  (T)x
        OpMultiplicative,   // *  /  %
        OpAdditive,         // +  -
        OpShift,            // <<  >>
        OpRelational,       // <  >  <=  >=  is  as
        OpEquality,         // ==  !=
        OpLogicalAnd,       // &
        //OpLogicalXor,       // ^
        OpLogicalOr,        // |
        OpConditionalAnd,   // &&
        OpConditionalOr,    // ||
        OpConditional,      // ?:
        OpAssignment,       // =  *=  /=  %=  +=  -=  <<=  >>=  &=  ^=  |=
        OpMin,              // the other delimiter to get the min priority number
    }
}
