using System;
using System.Collections.Generic;
using System.Text;

namespace PredicatesTests.ODataModels
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public int? FavoriteNumber { get; set; }
    }
}
