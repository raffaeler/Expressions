// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAmRaf.SimpleParser
{
    /// <summary>
    /// Represent a parameter
    /// </summary>
    public class Parameter
    {
        public Parameter(Type type, string name)
        {
            this.Type = type;
            this.Name = name;
        }

        public static Parameter Create<T>(string name)
        {
            return new Parameter(typeof(T), name);
        }

        public Type Type { get; set; }
        public string Name { get; set; }
    }
}
