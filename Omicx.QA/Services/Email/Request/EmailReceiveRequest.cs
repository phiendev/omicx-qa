using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.Entities;

namespace Omicx.QA.Services.Email.Request;

public class EmailReceiveRequest
{
    public Guid? Id { get; set; }
    public required string EmailId { get; set; }
    public Assignee? Assignee { get; set; }
    public string? RecordingUrl { get; set; }
    public string? Content { get; set; }
    public List<DynamicAttributeValue>? DynamicAttributeValues { get; set; }
}