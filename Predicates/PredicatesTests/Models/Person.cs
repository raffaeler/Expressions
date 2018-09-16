using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PredicatesTests.Models
{
    [DebuggerDisplay("Person: {Id} {Name} {Age} {Description}")]
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public short Age { get; set; }
    }
}
