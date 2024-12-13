using Omicx.QA.Entities;

namespace Omicx.QA.Services.Email.Dto;

public class EmailReceiveDto
{
    public Guid? Id { get; set; }
    public required string EmailId { get; set; }
    public Guid? TenantId { get; set; }
    public int? CustomTenantId { get; set; }
    public Assignee? Assignee { get; set; }
    public string? RecordingUrl { get; set; }
    public string? Content { get; set; }
    public Dictionary<string, string>? Links { get; set; }
    public List<EmailReceiveAttributeDto>? EmailReceiveAttributes { get; set; }
}