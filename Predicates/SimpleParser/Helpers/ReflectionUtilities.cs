// Copyright (c) 2017, Raffaele Rialdi
//

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;

//namespace IAmRaf.SimpleParser.Helpers
//{
//    internal static class ReflectionUtilities
//    {
//        private static MethodInfo _genericQueryableWhere2 = GenericMethodOf(_ => Queryable.Where<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));
//        private static MethodInfo _genericQueryableDistinct = GenericMethodOf(_ => Queryable.Distinct<int>(default(IQueryable<int>)));
//        private static MethodInfo _genericQueryableFirstOrDefault = GenericMethodOf(_ => Queryable.FirstOrDefault(default(IQueryable<int>)));

//        private static MethodInfo GenericMethodOf<TReturn>(Expression<Func<object, TReturn>> expression)
//        {
//            return GenericMethodOf(expression as Expression);
//        }

//        private static MethodInfo GenericMethodOf(Expression expression)
//        {
//            var lambdaExpression = expression as LambdaExpression;
//            var bodyCall = lambdaExpression.Body as MethodCallExpression;
//            var method = bodyCall.Method;
//            var typeDefinition = method.GetGenericMethodDefinition();
//            return typeDefinition;
//        }

//        public static MethodInfo GenericQueryableWhere2Parameters
//        {
//            get { return _genericQueryableWhere2; }
//        }

//        public static MethodInfo GenericQueryableDistinct
//        {
//            get { return _genericQueryableDistinct; }
//        }

//        public static MethodInfo GenericQueryableFirstOrDefault
//        {
//            get { return _genericQueryableFirstOrDefault; }
//        }
//    }
//}
