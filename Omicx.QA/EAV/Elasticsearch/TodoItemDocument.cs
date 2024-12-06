using Nest;
using Omicx.QA.Elasticsearch.Documents;

namespace Omicx.QA.EAV.Elasticsearch;

[ElasticsearchType(RelationName = "todo-items", IdProperty = nameof(Id))]
public class TodoItemDocument : ElasticNestedEntity
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
}