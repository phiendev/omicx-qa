using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.Entities.CallAggregate;
using Omicx.QA.Entities.EmailReceive;

namespace Omicx.QA.Services.Elastic;

public interface IDynamicEntityElasticService
{
    Task UpsertSchema(DynamicEntitySchema item);
    Task DeleteSchema(Guid id);
    Task SyncSchema(Guid? dynamicEntitySchemaId);
    Task UpsertCallAggregate(CallAggregate item, List<CallAggregateAttribute>? callAggregateAttributes);
    Task DeleteCallAggregate(Guid id);
    Task UpsertEmailReceive(EmailReceive item, List<EmailReceiveAttribute>? callAggregateAttributes);
    Task DeleteEmailReceive(Guid id);
}