using Omicx.QA.EAV.DynamicAttribute;

namespace Omicx.QA.Entities.CallAggregated;

public class CallAggregatedAttribute : EntityAttribute<CallAggregated>
{
    public virtual Guid? CallAggregatedId { get; set; }
}