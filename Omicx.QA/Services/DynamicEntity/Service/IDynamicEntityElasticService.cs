using Omicx.QA.EAV.DynamicEntity;

namespace Omicx.QA.Services.DynamicEntity.Service;

public interface IDynamicEntityElasticService
{
    Task UpsertSchema(DynamicEntitySchema item);
    Task DeleteSchema(Guid id);
}