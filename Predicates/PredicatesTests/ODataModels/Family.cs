using System;
using System.Collections.Generic;
using System.Text;

namespace PredicatesTests.ODataModels
{
    public class Family
    {
        public Family()
        {
            Persons = new List<Person>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Person> Persons { get; set; }
        public House Home { get; set; }
    }
}
