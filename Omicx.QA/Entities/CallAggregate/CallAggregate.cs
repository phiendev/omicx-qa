using MongoDB.Bson.Serialization.Attributes;
using Omicx.QA.JsonRequests;
using Omicx.QA.JsonRequests.Call;
using Volo.Abp.Domain.Entities.Auditing;

namespace Omicx.QA.Entities.CallAggregate;

public class CallAggregate : FullAuditedAggregateRoot<Guid> 
{
    public virtual required string CallId { get; set; }
    public virtual Guid? TenantId { get; set; }
    public virtual int? CustomTenantId { get; set; }
    public virtual Assignee? Assignee { get; set; }
    public virtual string? RecordingUrl { get; set; }
    public virtual string? Content { get; set; }
    public virtual Dictionary<string, string>? Links { get; set; }
    [BsonIgnore]
    public virtual List<CallAggregateAttribute>? CallAggregateAttributes { get; set; }
}