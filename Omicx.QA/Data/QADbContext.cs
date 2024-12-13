using MongoDB.Driver;
using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.Entities.CallAggregate;
using Omicx.QA.Entities.EmailReceive;
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
    public IMongoCollection<CallAggregate> CallAggregates => Collection<CallAggregate>();
    public IMongoCollection<CallAggregateAttribute> CallAggregateAttributes => Collection<CallAggregateAttribute>();
    public IMongoCollection<EmailReceive> EmailReceives => Collection<EmailReceive>();
    public IMongoCollection<EmailReceiveAttribute> EmailReceiveAttributes => Collection<EmailReceiveAttribute>();

    #region Dynamic attribute

    public IMongoCollection<DynamicEntitySchema> DynamicEntitySchemas => Collection<DynamicEntitySchema>();
    public IMongoCollection<AttributeGroup> AttributeGroups => Collection<AttributeGroup>();
    public IMongoCollection<DynamicAttribute> DynamicAttributes => Collection<DynamicAttribute>();

    #endregion
    
    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);
        
        modelBuilder.Entity<TodoItem>(b =>
        {
            b.CollectionName = "TodoItems";
        });
        modelBuilder.Entity<CallAggregate>(b =>
        {
            b.CollectionName = "CallAggregates";
        });
        modelBuilder.Entity<CallAggregateAttribute>(b =>
        {
            b.CollectionName = "CallAggregateAttributes";
        });
        modelBuilder.Entity<EmailReceive>(b =>
        {
            b.CollectionName = "EmailReceives";
        });
        modelBuilder.Entity<EmailReceiveAttribute>(b =>
        {
            b.CollectionName = "EmailReceiveAttributes";
        });
        modelBuilder.Entity<DynamicEntitySchema>(b =>
        {
            b.CollectionName = "DynamicEntitySchemas";
        });
        modelBuilder.Entity<AttributeGroup>(b =>
        {
            b.CollectionName = "AttributeGroups";
        });
        modelBuilder.Entity<DynamicAttribute>(b =>
        {
            b.CollectionName = "DynamicAttributes";
        });
    }
}

