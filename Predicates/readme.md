# Predicates solution

This is a solution containing three different ways to build a Linq Predicate.
All the libraries are netstandard and can be used with .NET Framework or .NET Core.

* OData: use the OData 4.0 specifications to convert a OData query to a linq expression.
* Predicates: build a predicate from scratch using this simple helper that allows to easily put together and/or conditions and to inject a behavior for true and/or false conditions
* SimpleParser: a simple text parser that parse a textual expression and converts it to a linq expression


Once obtained the expression it can be used with potentially every possible linq provider:
- Use the Compile method to obtain a delegate and execute the query in memory
- Use it with Entity Framework or other ORM to query the database. In this latter case be careful to use only the methods that the ORM supports


See the tests for the usage

Raf

### Releases

September 16, 2018
While this code is something I created a long time ago, this is the date of I released it on the wild
The very first release of the OData code and the predicate builder are dated several years ago with .NET 4.0.

Currently the 'expand' test is red. I still have to investigate whether the query is wrong or the my code is doing something wrong.


