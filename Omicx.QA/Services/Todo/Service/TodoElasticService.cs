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
        var index = ElasticsearchExtensions.GetIndexName<TodoItemDocument>(item.CustomTenantId);
        // var bulkResponse = await elasticClient.BulkAsync(x => x
        //     .Update<TodoItemDocument, TodoItemDocument>(u => u
        //         .Index(index)
        //         .Id(item.Id)
        //         .Doc(item)
        //         .Upsert(item)
        //     )
        // );

        var datas = new List<TodoItemDocument>
        {
            item
        }.ToArray();
        
        var bulkResponse =  await elasticClient.BulkAsync(x => x.UpdateMany(datas, (descriptor, entry) =>
        {
            descriptor.Index(index);
            descriptor.Doc(entry);
            return descriptor.Upsert(entry);
        }));
        
        //var bulkResponse = elasticClient.Index(item, i => i.Index(index));
        
        var sd = bulkResponse.IsValid;
    }
    
    public static async Task DeleteTodoItem(IElasticClient elasticClient, int? tenantId, long id)
    {
        if (tenantId is null) return;
        var index = ElasticsearchExtensions.GetIndexName<TodoItemDocument>(tenantId);
        var bulkResponse = await elasticClient.BulkAsync(x => x.Index(index)
            .Delete<TodoItemDocument>(d => d
                .Index(index)
                .Id(id)
            )
        );

        var sd = bulkResponse.IsValid;
    }
}