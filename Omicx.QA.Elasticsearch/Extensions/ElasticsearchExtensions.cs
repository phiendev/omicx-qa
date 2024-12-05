using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using Elasticsearch.Net;
using Nest;
using Omicx.QA.Elasticsearch.Configurations;
using Omicx.QA.Elasticsearch.Documents;
using Omicx.QA.Elasticsearch.Enums;
using Omicx.QA.Elasticsearch.Requests;

namespace Omicx.QA.Elasticsearch.Extensions;

public static class ElasticsearchExtensions
{
    public static IEnumerable<Type> GetTypesOfAttribute<T>(this AppDomain appDomain) where T : Attribute
    {
        return
            from a in appDomain.GetAssemblies().AsParallel()
            from t in a.GetTypes()
            let attributes = t.GetCustomAttributes(typeof(T), true)
            where attributes.Length > 0
            select t;
    }

    public static async Task<bool> CreateIndexAsync<TDoc>(
        this IElasticClient client, CancellationToken token = default) where TDoc : class
    {
        var esIndex = typeof(TDoc).GetCustomAttribute<ElasticsearchTypeAttribute>();
        if (esIndex == null
            || string.IsNullOrEmpty(esIndex.RelationName)
            || string.IsNullOrEmpty(esIndex.IdProperty))
            throw new ArgumentNullException(
                $"{nameof(esIndex.RelationName)} and {nameof(esIndex.IdProperty)} required!"
            );

        var type = typeof(TDoc).GetProperty(esIndex.IdProperty);
        if (type == null)
            throw new ArgumentNullException($"Cannot find property {typeof(TDoc)}.{esIndex.IdProperty}");

        var dataType = GetDataType(type.PropertyType);

        var existencyResponse = client.Indices.Exists(Indices.Index(esIndex.RelationName));
        if (existencyResponse.Exists)
        {
            return true;
        }

        var createRes = await client.LowLevel.Indices.CreateAsync<IndexResponse>(
            esIndex.RelationName,
            PostData.Serializable(new
            {
                settings = new
                {
                    analysis = new
                    {
                        char_filter = new { digits_only = new { type = "pattern_replace", pattern = "[^\\d]" } },
                        filter = new
                        {
                            _8_digits_min = new { type = "length", min = 8 },
                            not_empty = new { type = "length", min = 1 }
                        },
                        analyzer = new
                        {
                            accents = new
                            {
                                filter = new[] { "lowercase", "asciifolding" },
                                tokenizer = "whitespace",
                                type = "custom"
                            },
                            digits = new { tokenizer = "numeric_tokenizer" }
                        },
                        tokenizer =
                            new
                            {
                                numeric_tokenizer = new { type = "ngram", token_chars = new[] { "letter", "digit" } }
                            }
                    }
                },
                mappings = IndexMapping(client.FieldName(esIndex.IdProperty), dataType)
            }), ctx: token);

        return createRes.IsValid;
    }

    public static async Task<Responses.ISearchResponse<TDoc>> QueryAsync<TDoc>(
        this IElasticClient client,
        IElasticRequest request,
        CancellationToken token = default) where TDoc : class, IElasticNestedEntity
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        var searchResponse =
            await client.SearchAsync<IDictionary<string, object>>(sd => Query<TDoc>(client, request), token);
        if (!searchResponse.IsValid)
            throw new ElasticsearchClientException(searchResponse.DebugInformation);

        var totalCount = await CountAsync<TDoc>(client, request, token);
        var docResponse = Responses.SearchResponse<TDoc>.Of(totalCount)
            .Scroll(searchResponse.ScrollId);
        foreach (var doc in searchResponse.Documents)
        {
            var instance = Activator.CreateInstance<TDoc>();
            foreach (var (k, v) in doc) instance.Add(k, v);

            docResponse.Add(instance);
        }

        return docResponse;
    }

    public static async Task<TDocument> GetByIdAsync<TDocument>(this IElasticClient client,
        DocumentPath<IDictionary<string, object>> id,
        int? tenantId,
        CancellationToken token = default)
        where TDocument : class, IElasticNestedEntity
    {
        if (client == null) throw new ArgumentNullException(nameof(client));

        var indexName = GetIndexName<TDocument>(tenantId);

        var getResponse = await client.GetAsync(id, gd => gd.Index(indexName), ct: token);
        if (!getResponse.IsValid) throw new ElasticsearchClientException(getResponse.DebugInformation);

        var instance = Activator.CreateInstance<TDocument>();
        if (!getResponse.Found) return instance;

        foreach (var (k, v) in getResponse.Source) instance.Add(k, v);

        return instance;
    }

    public static async Task<long> CountAsync<TDoc>(
        this IElasticClient client,
        IElasticRequest request,
        CancellationToken token = default) where TDoc : class, IElasticNestedEntity
    {
        if (client == null) throw new ArgumentNullException(nameof(client));

        CountResponse? countRes = null;
        if (request is Requests.ISearchRequest search)
        {
            var esIndex = typeof(TDoc).GetCustomAttribute<ElasticsearchTypeAttribute>();

            var index = Indices.Index(
                $"{(request.TenantId != default ? $"{request.TenantId}_" : null)}{esIndex?.RelationName}");
            countRes = await client.CountAsync<TDoc>(cd =>
                    cd.Query(qcd =>
                    {
                        var query = search.Expression?.GetQuery();
                        return query ?? new QueryContainer();
                    }).Index(index),
                token
            );
        }

        if (countRes is { IsValid: true }) return countRes.Count;

        throw new ElasticsearchClientException(countRes?.DebugInformation);
    }

    public static async Task<IDictionary<string, long>> CountByFieldAsync<TDoc>(
        this IElasticClient client,
        IElasticRequest request,
        Expression<Func<TDoc, object>> groupByField
    ) where TDoc : class, IElasticNestedEntity
    {
        const string termAggregationName = "group_by_field";

        var esIndex = typeof(TDoc).GetCustomAttribute<ElasticsearchTypeAttribute>();
        var indexName = $"{(request.TenantId != default ? $"{request.TenantId}_" : null)}{esIndex?.RelationName}";

        var (returnType, name) = GetFieldName(groupByField);
        var fieldName = client.ConnectionSettings?.DefaultFieldNameInferrer?
            .Invoke(typeof(string) == returnType ? $"{name}.raw" : name);

        var searchDescriptor = new SearchDescriptor<object>().Index(indexName)
            .Size(0)
            .Aggregations(a => a
                .Terms(termAggregationName, st => st
                    .Field(fieldName)
                )
            );

        searchDescriptor.Query(qcd =>
        {
            var query = request is Requests.ISearchRequest { Expression: { } } search
                ? search.Expression?.GetQuery()
                : new QueryContainer();
            return query;
        });

        var response = await client.SearchAsync<object>(searchDescriptor);
        if (!response.IsValid)
            throw new ElasticsearchClientException(response.DebugInformation);

        var result = response.Aggregations.Terms(termAggregationName).Buckets
            .ToDictionary(item => item.Key, item => item.DocCount ?? 0);

        return result;
    }

    public static async Task<Responses.ISearchResponse<TDoc>> ScrollDocumentAsync<TDoc>(
        this IElasticClient client,
        string scroll,
        string scrollId,
        CancellationToken token = default) where TDoc : class, IElasticNestedEntity
    {
        var scrollResponse = await client.ScrollAsync<IDictionary<string, object>>(scroll, scrollId, ct: token);

        if (scrollResponse.IsValid)
        {
            var docResponse = Responses.SearchResponse<TDoc>.Create();
            foreach (var doc in scrollResponse.Documents)
            {
                var instance = Activator.CreateInstance<TDoc>();
                foreach (var (k, v) in doc) instance.Add(k, v);

                docResponse.Add(instance);
            }

            return docResponse;
        }

        throw new ElasticsearchClientException(scrollResponse.DebugInformation);
    }

    public static SearchDescriptor<IDictionary<string, object>>
        GetSearchDescriptor<TDoc>(this IElasticClient client, IElasticRequest request)
        where TDoc : class, IElasticNestedEntity
    {
        return Query<TDoc>(client, request);
    }

    private static SearchDescriptor<IDictionary<string, object>>
        Query<TDoc>(this IElasticClient client, IElasticRequest request) where TDoc : class, IElasticNestedEntity
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var index = GetIndexName<TDoc>(request.TenantId);

        var searchDescriptor = new SearchDescriptor<IDictionary<string, object>>().Index(index);

        if (request is Requests.ISearchRequest { Expression: { } } search)
        {
            searchDescriptor.Query(qcd => search.Expression.GetQuery());
        }
        else
        {
            searchDescriptor.Query(qcd => qcd);
        }

        if (!(request is IPageRequest page)) return searchDescriptor;

        searchDescriptor.Scroll(page.ScrollTime);

        if (page.Size == null) searchDescriptor.Skip(page.Skip).Take(page.Take);

        if (!page.HasSort) return searchDescriptor;


        var sortDescriptor = new SortDescriptor<IDictionary<string, object>>();
        foreach (var asc in page.OrderBy) sortDescriptor.Ascending(FieldName(client, asc));
        foreach (var desc in page.OrderByDesc) sortDescriptor.Descending(FieldName(client, desc));

        searchDescriptor.Sort(s => sortDescriptor);

        if (page.SortingExpressions.Any())
        {
            searchDescriptor.Sort(s =>
                page.SortingExpressions.Aggregate(s, (current, expression) => expression(current)));
        }

        if (page.Size != null)
        {
            searchDescriptor.Size(page.Size);
        }

        if (page.ListSearchAfter.Any())
        {
            searchDescriptor.SearchAfter(page.ListSearchAfter);
        }

        return searchDescriptor;
    }

    public static string FieldName(this IElasticClient client, string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) return "";

        return client.ConnectionSettings?.DefaultFieldNameInferrer?.Invoke(fieldName) ?? fieldName;
    }

    private static object IndexMapping(string idProperty, string dataType) => new
    {
        properties = new Dictionary<string, object> { { idProperty, new { type = dataType } } },
        dynamic_templates = new object[]
        {
            new
            {
                string_accents_and_raw = new
                {
                    match_mapping_type = "string",
                    mapping = new
                    {
                        type = "text",
                        analyzer = "accents",
                        fields = new { raw = new { type = "keyword", ignore_above = 256 } }
                    }
                }
            },
            new
            {
                numeric_full_text = new
                {
                    match_mapping_type = "string",
                    match_pattern = "regex",
                    match = "(.*)(phone)(.*)",
                    mapping = new { type = "text", analyzer = "digits" }
                }
            }
        }
    };


    public static IMappingExpression<TSrc, TDes>? DynamicAttributes<TSrc, TDes, TProp>(
        this IMappingExpression<TSrc, TDes>? mapping,
        Expression<Func<TSrc, ICollection<TProp>>>? attributesExp)
        where TDes : ElasticNestedEntity
        where TProp : IDynamicAttribute
    {
        if (mapping == null || attributesExp == null) return null;
        return mapping.AfterMap((src, des) =>
        {
            var getter = attributesExp.Compile();
            var attributes = getter(src);
            foreach (var attribute in attributes)
            {
                var (key, val) = attribute.GetProperty();
                des.Add(key, val);
            }

            des.AfterPropertiesSet();
        });
    }


    public static string UpperFirst(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };

    public static DataType ConvertToDataType(this Type type)
    {
        var dataType = Nullable.GetUnderlyingType(type) ?? type;
        var typeCode = Type.GetTypeCode(dataType);

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            var dt = elementType?.ConvertToDataType();

            return DataType.Object.Equals(dt) ? DataType.Nested : DataType.Select;
        }

        switch (typeCode)
        {
            case TypeCode.Empty:
                return DataType.Null;
            case TypeCode.String:
            case TypeCode.Char:
                return DataType.Text;
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Double:
            case TypeCode.Single:
            case TypeCode.Decimal:
                return DataType.Number;
            case TypeCode.DateTime:
                return DataType.DateTime;
            case TypeCode.Boolean:
                return DataType.Boolean;
            case TypeCode.Object:
                if (!typeof(IEnumerable).IsAssignableFrom(type))
                    return dataType == typeof(DateTimeOffset) ? DataType.DateTime : DataType.Object;

                var listItemType = type.GetGenericArguments().FirstOrDefault();

                var dt = listItemType?.ConvertToDataType();

                return DataType.Object.Equals(dt)
                    ? DataType.Nested
                    : dt ?? DataType.Text;
            default:
                return DataType.Text;
        }
    }

    private static string GetDataType(Type type)
    {
        if (type == typeof(long)) return "long";

        if (type == typeof(int)) return "integer";

        if (type == typeof(short)) return "short";

        return "text";
    }

    public static IndexName GetIndexName<TDocument>(int? tenantId)
    {
        var esIndex = typeof(TDocument).GetCustomAttribute<ElasticsearchTypeAttribute>();

        return Indices.Index(
            $"{(tenantId != default ? $"{tenantId}_" : null)}{esIndex?.RelationName}");
    }

    public static (Type?, string?) GetFieldName<TDoc>(Expression<Func<TDoc, object>> exp)
    {
        if (exp.Body is MethodCallExpression mcExp)
        {
            var lastArg = mcExp.Arguments.LastOrDefault();
            if (lastArg == null) return (null, null);

            var constantExp = (ConstantExpression)lastArg;

            return (constantExp.Type, (string?)constantExp.Value);
        }

        var memberExpression = exp.Body is UnaryExpression unaryExp
            ? (MemberExpression)unaryExp.Operand
            : (MemberExpression)exp.Body;

        var propInfo = (PropertyInfo)memberExpression.Member;

        return (propInfo.GetMethod?.ReturnType, propInfo.Name);
    }
}

// public static class ElasticsearchConfigurationExtensions
// {
//     public static IElasticsearchConfiguration Elasticsearch(this IModuleConfigurations configurations)
//     {
//         return configurations.AbpConfiguration.Get<IElasticsearchConfiguration>();
//     }
// }