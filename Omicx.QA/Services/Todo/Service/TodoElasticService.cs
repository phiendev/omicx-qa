using Nest;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Elasticsearch.Extensions;
using Omicx.QA.Entities.Todo;

namespace Omicx.QA.Services.Todo.Service;

public static class TodoElasticService
{
    public static async Task UpsertTodoItem(IElasticClient elasticClient, TodoItemDocument item)
    {
        if (item.CustomTenantId is null) return;
        item.AfterPropertiesSet();
        var index = ElasticsearchExtensions.GetIndexName<TodoItemDocument>(item.CustomTenantId);
        var bulkResponse = await elasticClient.BulkAsync(x => x
            .Index(index)
            .Update<TodoItemDocument>(u => u
                .Id(item.Id)
                .Doc(item)
                .DocAsUpsert(true)
            )
        );
        if (!bulkResponse.IsValid)
        {
            Console.WriteLine(bulkResponse.DebugInformation);
        }
    }
    
    public static async Task DeleteTodoItem(IElasticClient elasticClient, int? tenantId, Guid id)
    {
        if (tenantId is null) return;
        var index = ElasticsearchExtensions.GetIndexName<TodoItemDocument>(tenantId);
        var bulkResponse = await elasticClient.BulkAsync(x => x.Index(index)
            .Delete<TodoItemDocument>(d => d
                .Index(index)
                .Id(id)
            )
        );
        if (!bulkResponse.IsValid)
        {
            Console.WriteLine(bulkResponse.DebugInformation);
        }
    }
}