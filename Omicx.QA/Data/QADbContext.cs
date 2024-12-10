using MongoDB.Driver;
using Omicx.QA.Entities.CallAggregated;
using Omicx.QA.Entities.Todo;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace Omicx.QA.Data;

[ConnectionStringName("Default")]
public class QADbContext : AbpMongoDbContext
{   
    /* Add mongo collections here. Example:
     * public IMongoCollection<Question> Questions => Collection<Question>();
     */
    public IMongoCollection<TodoItem> TodoItems => Collection<TodoItem>();
    public IMongoCollection<CallAggregated> CallAggregateds => Collection<CallAggregated>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);
        
        modelBuilder.Entity<TodoItem>(b =>
        {
            b.CollectionName = "TodoItems";
        });
        modelBuilder.Entity<CallAggregated>(b =>
        {
            b.CollectionName = "CallAggregateds";
        });
    }
}

