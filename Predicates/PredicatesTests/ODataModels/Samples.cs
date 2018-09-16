using System;
using System.Collections.Generic;
using System.Text;

namespace PredicatesTests.ODataModels
{
    public static class Samples
    {
        public static IList<Family> GetSampleData()
        {
            var families = new List<Family>();
            var family = new Family()
            {
                Id = 1,
                Name = "Da Family",
                Home = new House()
                {
                    Id = 1,
                    Address = "White st.",
                    Color = Color.Red,
                },
            };

            family.Persons.Add(new Person()
            {
                Id = 1,
                Name = "Zot",
                BirthDate = DateTime.Now.AddYears(-10),
                FavoriteNumber = 0,
            });

            family.Persons.Add(new Person()
            {
                Id = 2,
                Name = "Bang",
                BirthDate = DateTime.Now.AddYears(-40),
                FavoriteNumber = 3,
            });

            family.Persons.Add(new Person()
            {
                Id = 3,
                Name = "Boom",
                BirthDate = DateTime.Now.AddYears(-35),
                FavoriteNumber = null,
            });

            families.Add(family);

            family = new Family()
            {
                Id = 2,
                Name = "Da Cousins",
                Home = new House()
                {
                    Id = 2,
                    Address = "Black st.",
                    Color = Color.Blue,
                },
            };

            family.Persons.Add(new Person()
            {
                Id = 4,
                Name = "Jack",
                BirthDate = DateTime.Now.AddYears(-15),
                FavoriteNumber = 10,
            });

            family.Persons.Add(new Person()
            {
                Id = 5,
                Name = "Carl",
                BirthDate = DateTime.Now.AddYears(-37),
                FavoriteNumber = 10,
            });

            family.Persons.Add(new Person()
            {
                Id = 6,
                Name = "Sandra",
                BirthDate = DateTime.Now.AddYears(-41),
                FavoriteNumber = 10,
            });

            families.Add(family);
            return families;
        }
    }
}
