using System;
using System.Collections.Generic;
using System.Text;

namespace PredicatesTests.Models
{
    public static class Samples
    {
        public static IList<Person> GetSampleData()
        {
            return new List<Person>()
            {
                new Person()
                {
                    Id = 1,
                    Name = "Ben",
                    Description = "the first one",
                    Age = 50,
                },
                new Person()
                {
                    Id = 2,
                    Name = "Daniel",
                    Description = "random guy",
                    Age = 22,
                },
                new Person()
                {
                    Id = 3,
                    Name = "Stella",
                    Description = "someone",
                    Age = 27,
                },
                new Person()
                {
                    Id = 4,
                    Name = "Lisa",
                    Description = "someone else",
                    Age = 32,
                },
            };
        }
    }
}
