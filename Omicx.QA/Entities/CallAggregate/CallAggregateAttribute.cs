using Omicx.QA.EAV.DynamicAttribute;

namespace Omicx.QA.Entities.CallAggregate;

public class CallAggregateAttribute : EntityAttribute<CallAggregate>
{
    public virtual Guid? CallAggregateId { get; set; }
}