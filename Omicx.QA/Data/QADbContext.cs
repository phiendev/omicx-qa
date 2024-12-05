using MongoDB.Driver;
using Omicx.QA.Entities;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace Omicx.QA.Data;

[ConnectionStringName("Default")]
public class QADbContext : AbpMongoDbContext
{   
    /* Add mongo collections here. Example:
     * public IMongoCollection<Question> Questions => Collection<Question>();
     */
    
    public IMongoCollection<TodoItem> TestItems => Collection<TodoItem>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.Entity<TodoItem>(b =>
        {
            b.CollectionName = "TodoItem";
        });
    }
}

