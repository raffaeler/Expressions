// Copyright (c) 2017, Raffaele Rialdi
//
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;

namespace ODataPredicates
{
    public class ODataPredicateBuilder
    {
        private static ConcurrentDictionary<Type, IEdmModel> _cachedModels = new ConcurrentDictionary<Type, IEdmModel>();
        private readonly IEnumerable<Type> _modelTypes;
        private IServiceProvider _serviceProvider;
        private IEdmModel _model = null;

        public ODataPredicateBuilder()
            : this(new Type[] { })
        {
        }

        public ODataPredicateBuilder(Type modelType)
            : this(new Type[] { modelType }.AsEnumerable())
        {
        }

        public ODataPredicateBuilder(params Type[] modelTypes)
            : this(modelTypes.AsEnumerable())
        {
        }

        public ODataPredicateBuilder(IEnumerable<Type> modelTypes)
        {
            _modelTypes = modelTypes;

            InitializeServices();
            HandleNullPropagation = true;
        }

        /// <summary>
        /// After creating the expression the locals are evaluated to avoid references
        /// to a captured expression.
        /// This is very useful to simplify the expression and mandatory when the expression
        /// is translated to a database
        /// </summary>
        public bool EvaluateLocals { get; set; }

        /// <summary>
        /// Generates an expressiono that check for null before accessing the entity
        /// It is needed to execute a query in memory and avoid potential NullReferenceException
        /// Default is true
        /// </summary>
        public bool HandleNullPropagation { get; set; }

        public bool UseCachedModels { get; set; }

        public Expression GetExpressionFromString(Type rootType, Type entityType,
            string filter, string orderby = null, string selectExpand = null)
        {
            var queryContext = GetQueryContext(rootType, entityType);
            var parser = CreateParser(rootType, entityType, filter, orderby, selectExpand);
            var queryable = CreateFakeQueryable(rootType);
            var expression = ApplyOptions(queryable, queryContext, parser, filter, orderby, selectExpand);

            return expression;
        }

        // new ok
        private IQueryable CreateFakeQueryable(Type entityType)
        {
            var listTType = typeof(List<>).MakeGenericType(entityType);
            var fake = (Activator.CreateInstance(listTType) as System.Collections.IList).AsQueryable();
            return fake;
        }

        // new ok
        private Expression ApplyOptions(IQueryable queryable, ODataQueryContext queryContext,
            Microsoft.OData.UriParser.ODataQueryOptionParser queryOptionParser,
            string filter, string orderby, string selectExpand)
        {
            var filterQueryOption = filter == null ? null : new FilterQueryOption(filter, queryContext, queryOptionParser);
            var orderQueryOption = orderby == null ? null : new OrderByQueryOption(orderby, queryContext, queryOptionParser);
            var selectExpandQueryOption = selectExpand == null ? null : new SelectExpandQueryOption(null, selectExpand, queryContext, queryOptionParser);

            var querySettings = new ODataQuerySettings()
            {
                HandleNullPropagation = this.HandleNullPropagation ?
                    HandleNullPropagationOption.True :
                    HandleNullPropagationOption.False,            
            };

            var validationSettings = new ODataValidationSettings() { MaxNodeCount = 100 };
            try
            {
                filterQueryOption?.Validate(validationSettings);
                orderQueryOption?.Validate(validationSettings);
                selectExpandQueryOption?.Validate(validationSettings);
            }
            catch (Exception)
            {
                throw;
            }

            IQueryable result = queryable;
            if(filterQueryOption != null) result = filterQueryOption?.ApplyTo(result, querySettings);
            if (orderQueryOption != null) result = orderQueryOption?.ApplyTo(result, querySettings);
            if (selectExpandQueryOption != null) result = selectExpandQueryOption?.ApplyTo(result, querySettings);
            var expression = result.Expression;
            if (EvaluateLocals)
            {
                expression = CapturedEvaluator.CapturedEvaluator.Eval(expression);
            }

            return expression;
        }

        // new, ok
        private Microsoft.OData.UriParser.ODataQueryOptionParser CreateParser(
            Type rootType, Type entityType,
            string filter = null, string orderby = null, string selectExpand = null)
        {
            var edmType = _model.FindDeclaredType(rootType.FullName);

            var clauses = new Dictionary<string, string>()
            {
                {"$filter", filter},
                {"$orderby", orderby},
                {"$select", null},
                {"$expand", selectExpand},
            };

            var parser = new Microsoft.OData.UriParser.ODataQueryOptionParser(_model, edmType, null, clauses, _serviceProvider)
            {
                Resolver = new Microsoft.OData.UriParser.StringAsEnumResolver() { EnableCaseInsensitive = true, },
                Settings = { MaximumExpansionDepth = 1, },
            };

            return parser;
        }

        // new, ok
        private ODataQueryContext GetQueryContext(Type rootType, Type entityType)
        {
            MakeModel(rootType);
            //var entitySet = _model.FindDeclaredEntitySet(rootType.Name);
            var segment = new Microsoft.OData.UriParser.PathTemplateSegment($"~/"); // {rootType.Name}

            var path = new ODataPath(segment);
            var uriParseODataPath = new Microsoft.OData.UriParser.ODataPath(segment);
            var queryContext = new ODataQueryContext(_model, entityType, path)
            {
                DefaultQuerySettings =
                {
                    EnableCount = true,
                    EnableExpand = true,
                    EnableFilter = true,
                    EnableOrderBy = true,
                    EnableSelect = true,
                    //MaxTop = 100,
                },
            };

            return queryContext;
        }

        // new, ok
        private void MakeModel(Type rootType)
        {
            if (_model != null)
            {
                return;
            }

            if (UseCachedModels)
            {
                if (!_cachedModels.TryGetValue(rootType, out IEdmModel model))
                {
                    var builder = GetModelBuilder(rootType);
                    model = builder.GetEdmModel();
                    _cachedModels[rootType] = model;
                }

                _model = model;
            }
            else
            {
                var builder = GetModelBuilder(rootType);
                _model = builder.GetEdmModel();
            }
        }

        // new ok
        private ODataModelBuilder GetModelBuilder(Type rootType)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(_serviceProvider);

            AddEntityType(builder, rootType);

            //var hierarchyTypes = SimaticIT.CommonHelpers.ReflectionUtilities.GetTypesByBaseType(rootType);
            //foreach (var type in hierarchyTypes)
            //{
            //    AddEntityType(builder, type);
            //}

            foreach (var type in _modelTypes)
            {
                if (type != rootType)
                {
                    AddEntityType(builder, type);
                }
            }

            return builder;
        }


        // TODO: customize the way the key is identified
        private void AddEntityType(ODataModelBuilder builder, Type type)
        {
            var properties = type.GetProperties();
            var key = properties.Where(p => p.PropertyType == typeof(Guid)).FirstOrDefault();
            if (key == null)
            {
                key = properties.First();
            }

            var edmSetConfig = builder.AddEntitySet(type.Name, new EntityTypeConfiguration(builder, type));
            var edmTypeConfig = builder.AddEntityType(type);
            edmTypeConfig.HasKey(key);
        }

        private void InitializeServices()
        {
            var containerBuilder = new DefaultContainerBuilder();

            containerBuilder.AddService(
                Microsoft.OData.ServiceLifetime.Scoped,
                typeof(Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartManager),
                typeof(Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartManager));

            containerBuilder.AddService(
                Microsoft.OData.ServiceLifetime.Scoped,
                typeof(ODataQuerySettings), sp =>
                    new ODataQuerySettings()
                    {
                        HandleNullPropagation = this.HandleNullPropagation ?
                        HandleNullPropagationOption.True :
                        HandleNullPropagationOption.False,
                    });

            containerBuilder.AddService(
                Microsoft.OData.ServiceLifetime.Scoped,
                typeof(IEdmModel), sp =>
                {
                    return _model;
                });

            containerBuilder.AddService(
                Microsoft.OData.ServiceLifetime.Scoped,
                typeof(Microsoft.OData.UriParser.ODataUriResolver), sp =>
                {
                    return new Microsoft.OData.UriParser.ODataUriResolver()
                    {
                    };
                });

            containerBuilder.AddService(
                Microsoft.OData.ServiceLifetime.Scoped,
                typeof(Microsoft.OData.ODataSimplifiedOptions), sp =>
                {
                    return new Microsoft.OData.ODataSimplifiedOptions()
                    {
                    };
                });

            containerBuilder.AddService(
                Microsoft.OData.ServiceLifetime.Scoped,
                typeof(Microsoft.OData.UriParser.ODataUriParserSettings), sp =>
                {
                    return new Microsoft.OData.UriParser.ODataUriParserSettings()
                    {
                    };
                });

            _serviceProvider = containerBuilder.BuildContainer();
        }

        private class ServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                return Activator.CreateInstance(serviceType);
            }
        }

        /*
         * Old implementation
        
        private FilterQueryOption CreateFilterQueryOption0b(string oDataFilter, Type rootType, Type entityType)
        {
            MakeModel(rootType);
            var edmType = _model.FindDeclaredType(rootType.FullName);
            var entitySet = _model.FindDeclaredEntitySet(rootType.Name);
            var segment = new Microsoft.OData.UriParser.EntitySetSegment(entitySet);

            //new Microsoft.OData.UriParser.ODataPathSegment("fakeSetName");

            var path = new ODataPath(segment);
            var uriParseODataPath = new Microsoft.OData.UriParser.ODataPath(segment);
            var queryContext = new ODataQueryContext(_model, entityType, path);


            var clauses = new Dictionary<string, string>()
            {
                {"$filter", oDataFilter},
            };

            var schemaType = _model.FindDeclaredType(rootType.FullName);
            var parser = new Microsoft.OData.UriParser.ODataQueryOptionParser(_model, edmType, null, clauses, _serviceProvider);
            var parsed = parser.ParseFilter();  // test
            var singleValueNode = parsed.Expression.Accept(new ParameterAliasNodeTranslator(parser.ParameterAliasNodes)) as Microsoft.OData.UriParser.SingleValueNode;
            singleValueNode = (singleValueNode ?? new Microsoft.OData.UriParser.ConstantNode(null));
            var clause = new Microsoft.OData.UriParser.FilterClause(singleValueNode, parsed.RangeVariable);
            var filterQueryOption = new FilterQueryOption(oDataFilter, queryContext, parser);
            return filterQueryOption;
        }


        // old but works
        private FilterQueryOption CreateFilterQueryOption(string oDataFilter, Type rootType, Type entityType)
        {
            MakeModel(rootType);
            var edmType = _model.FindDeclaredType(rootType.FullName);
            //var entitySet = _model.FindDeclaredEntitySet(rootType.Name);
            var segment = new Microsoft.OData.UriParser.PathTemplateSegment($"~/"); // {rootType.Name}

            //new Microsoft.OData.UriParser.ODataPathSegment("fakeSetName");

            var path = new ODataPath(segment);
            var uriParseODataPath = new Microsoft.OData.UriParser.ODataPath(segment);
            var queryContext = new ODataQueryContext(_model, entityType, path);


            var clauses = new Dictionary<string, string>()
            {
                {"$filter", oDataFilter},
            };

            //var schemaType = _model.FindDeclaredType(rootType.FullName);
            var parser = new Microsoft.OData.UriParser.ODataQueryOptionParser(_model, edmType, null, clauses, _serviceProvider);
            //var parsed = parser.ParseFilter();  // test
            //var singleValueNode = parsed.Expression.Accept(new ParameterAliasNodeTranslator(parser.ParameterAliasNodes)) as Microsoft.OData.UriParser.SingleValueNode;
            //singleValueNode = (singleValueNode ?? new Microsoft.OData.UriParser.ConstantNode(null));

            //var clause = new Microsoft.OData.UriParser.FilterClause(singleValueNode, parsed.RangeVariable);
            var filterQueryOption = new FilterQueryOption(oDataFilter, queryContext, parser);
            return filterQueryOption;
        }



        private FilterQueryOption CreateFilterQueryOption1(string oDataFilter, Type rootType, Type entityType)
        {
            var queryContext = GetQueryContext_old(rootType, entityType);
            var clauses = new Dictionary<string, string>()
            {
                {"$filter", oDataFilter},
            };

            var schemaType = _model.FindDeclaredType(rootType.FullName);
            var parser = new Microsoft.OData.UriParser.ODataQueryOptionParser(_model, schemaType, null, clauses);
            var filterQueryOption = new FilterQueryOption(oDataFilter, queryContext, parser);
            return filterQueryOption;
        }

        // old
        private ODataQueryContext GetQueryContext_old(Type rootType, Type entityType)
        {
            MakeModel(rootType);
            var segment = new Microsoft.OData.UriParser.PathTemplateSegment($"~/{rootType.Name}");
            var path = new ODataPath(segment);
            var ctx = new ODataQueryContext(_model, entityType, path);
            return ctx;
        }

        public Expression GetExpressionFromString_ITWORKS(Type rootType, Type entityType,
            string filter, string orderby = null, string selectExpand = null)
        {
            var filterQueryOption = CreateFilterQueryOption(filter, rootType, entityType);
            var querySettings = new ODataQuerySettings()
            {
                HandleNullPropagation = this.HandleNullPropagation ?
                    HandleNullPropagationOption.True :
                    HandleNullPropagationOption.False,
            };

            var listTType = typeof(List<>).MakeGenericType(entityType);
            var fake = (Activator.CreateInstance(listTType) as System.Collections.IList).AsQueryable();
            var filtered = filterQueryOption.ApplyTo(fake.AsQueryable(), querySettings);
            var filteredExpression = filtered.Expression;
            if (EvaluateLocals)
            {
                filteredExpression = CapturedEvaluator.CapturedEvaluator.Eval(filteredExpression);
            }

            return filteredExpression;
        }

        public Expression GetExpressionFromString_0(string odataString,
            Type rootType, Type entityType)
        {
            MakeModel(rootType);

            Uri fakeRoot = new Uri("http://fake/");
            Uri uri = new Uri($"{rootType.Name}?$filter={odataString}", UriKind.Relative);
            var parser = new Microsoft.OData.UriParser.ODataUriParser(_model, fakeRoot, uri);
            var filter = parser.ParseFilter();
            var singleValueNode = filter.Expression.Accept(new ParameterAliasNodeTranslator(parser.ParameterAliasNodes))
                as Microsoft.OData.UriParser.SingleValueNode;
            singleValueNode = (singleValueNode ?? new Microsoft.OData.UriParser.ConstantNode(null));
            var clause = new Microsoft.OData.UriParser.FilterClause(singleValueNode, filter.RangeVariable);


            //var fqo = new FilterQueryOption(odataString, )


            var filterBinder = new Microsoft.AspNet.OData.Query.Expressions.FilterBinder(_serviceProvider);
            var expression = filterBinder.Bind(singleValueNode);
            return expression;

            ////var entitySet = _model.FindDeclaredEntitySet(rootType.Name);
            //var segment = new Microsoft.OData.UriParser.PathTemplateSegment($"~/{rootType.Name}");

            ////new Microsoft.OData.UriParser.ODataPathSegment("fakeSetName");

            //var path = new ODataPath(segment);
            //var uriParseODataPath = new Microsoft.OData.UriParser.ODataPath(segment);
            //var queryContext = new ODataQueryContext(_model, entityType, path);


            //var clauses = new Dictionary<string, string>()
            //{
            //    {"$filter", oDataFilter},
            //};

            //var schemaType = _model.FindDeclaredType(rootType.FullName);
            //var parser = new Microsoft.OData.UriParser.ODataQueryOptionParser(_model, uriParseODataPath, clauses);
            //var parsed = parser.ParseFilter();  // test
            //var singleValueNode = parsed.Expression.Accept(new ParameterAliasNodeTranslator(parser.ParameterAliasNodes)) as Microsoft.OData.UriParser.SingleValueNode;
            //singleValueNode = (singleValueNode ?? new Microsoft.OData.UriParser.ConstantNode(null));
            //var clause = new Microsoft.OData.UriParser.FilterClause(singleValueNode, parsed.RangeVariable);
            //var filterQueryOption = new FilterQueryOption(oDataFilter, queryContext, parser);
            //return filterQueryOption;
        }


         */
    }
}
