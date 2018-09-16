using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IAmRaf.SimpleParser;

using Predicates.Injector;
using Predicates.PredicateBuilder;

namespace TestConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Program();
            p.PredicateBuilderTest1();
            //p.Demo1();
            //p.Demo2();
            p.PerfTest();
        }

        private void PerfTest()
        {
            int num = 1000;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < num; i++)
            {
                var builder = new PredicateBuilder();
                var parameter = Expression.Parameter(typeof(string), "s");
                var condition = builder.And<string>(parameter,
                    s => s.Length > 3);
                var func = builder.CreateLambdaPredicate<string>(condition, parameter);
                Debug.Assert(func != null);
            }

            Console.WriteLine($"Total time for {num} loops: {sw.ElapsedMilliseconds}");
        }

        private void Demo1()
        {
            ParseResult e;
            // f(x, y) = x + y
            e = SimpleParser.Parse("x+ y",
                new Parameter(typeof(int), "x"),
                new Parameter(typeof(int), "y"));
            var f1 = e.Compile<Func<int, int, int>>();

            // f(x) = x^2 + 2*x + 1
            e = SimpleParser.Parse("x^2 + 2*x + 1",
                typeof(float),
                new Parameter(typeof(float), "x"));
            var f2 = e.Compile<Func<float, float>>();

            // f(x) = x + SUM(x- 1, 3, 2) - 2
            e = SimpleParser.Parse("x + SUM(x- 1, 3, 2) - 2",
                new Parameter(typeof(double), "x"));
            var f3 = e.Compile<Func<double, double>>();

            // f(x) = (x == "raf")    [predicate]
            e = SimpleParser.Parse("x == \"raf\"", Parameter.Create<string>("x"));
            var f4 = e.Compile<Func<string, bool>>();
        }

        private void Demo2()
        {
            Expression<Func<int, bool>> predicate = GetFilter();

            var injected = Injector2.Inject(predicate,
                x => Console.WriteLine($"{x} => YES"),
                x => Console.WriteLine($"{x} => NO"));
            var injectedDel = injected.Compile();

            var list = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var filtered = list
                .Where(injectedDel)
                .ToList();
        }

        private Expression<Func<int, bool>> GetFilter()
        {
            return p => p % 2 == 0;
        }


        public Func<Person, bool> CreateFilter(string name, int age, bool isAnd)
        {
            var builder = new PredicateBuilder();
            var parameter = Expression.Parameter(typeof(Person), "common");

            Expression condition;
            if (isAnd)
            {
                condition = builder.And<Person>(parameter,
                    p => p.Name == name,
                    p => p.Age == age);
            }
            else
            {
                condition = builder.Or<Person>(parameter,
                    p => p.Name == name,
                    p => p.Age == age);
            }

            var lambda = builder.CreateLambdaPredicate<Person>(
                condition, parameter);
            var predicate = lambda.Compile();
            return predicate;
        }

        public IList<Person> FilterPersonsByNameAndAge(string name, int age, bool isAnd)
        {
            var persons = Samples.GetSampleData();
            var predicate = CreateFilter(name, age, isAnd);
            var result = persons
                .Where(p => predicate(p))
                .ToList();

            return result;
        }

        public void PredicateBuilderTest1()
        {
            var isAnd = true;
            //int age = 22;
            //string name = "Daniel";

            var persons = Samples.GetSampleData();
            var builder = new PredicateBuilder();
            var parameter = Expression.Parameter(typeof(Person), "common");

            Expression condition;
            if (isAnd)
            {
                condition = builder.And<Person>(parameter,
                    p => p.Name == "Daniel",
                    p => p.Age == 22);
            }
            else
            {
                condition = builder.Or<Person>(parameter,
                    p => p.Name == "Daniel",
                    p => p.Age == 22);
            }

            var lambda = builder.CreateLambdaPredicate<Person>(
                condition, parameter);
            var predicate = lambda.Compile();
            var filtered = persons.Where(predicate).ToList();

            //Assert.IsTrue(filtered.Count == 1);
        }
    }

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

    [DebuggerDisplay("Person: {Id} {Name} {Age} {Description}")]
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public short Age { get; set; }
    }


}
