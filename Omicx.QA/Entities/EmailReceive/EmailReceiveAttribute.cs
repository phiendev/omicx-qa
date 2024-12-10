using Omicx.QA.EAV.DynamicAttribute;

namespace Omicx.QA.Entities.EmailReceive;

public class EmailReceiveAttribute : EntityAttribute<EmailReceive>
{
    public virtual Guid? EmailReceiveId { get; set; }
}