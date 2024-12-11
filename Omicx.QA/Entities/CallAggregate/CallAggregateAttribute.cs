using Omicx.QA.EAV.DynamicEntity;

namespace Omicx.QA.Entities.CallAggregate;

public class CallAggregateAttribute : EntityAttribute<CallAggregate>
{
    public virtual Guid? CallAggregateId { get; set; }
}