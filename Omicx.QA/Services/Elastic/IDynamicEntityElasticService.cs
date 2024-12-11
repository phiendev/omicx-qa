using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.Entities.CallAggregate;

namespace Omicx.QA.Services.Elastic;

public interface IDynamicEntityElasticService
{
    Task UpsertSchema(DynamicEntitySchema item);
    Task DeleteSchema(Guid id);
    Task SyncSchema(Guid? dynamicEntitySchemaId);
    Task UpsertCallAggregate(CallAggregate item);
}