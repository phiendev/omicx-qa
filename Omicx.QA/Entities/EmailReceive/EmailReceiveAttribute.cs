using Omicx.QA.EAV.DynamicEntity;

namespace Omicx.QA.Entities.EmailReceive;

public class EmailReceiveAttribute : EntityAttribute<EmailReceive>
{
    public virtual Guid? EmailReceiveId { get; set; }
}