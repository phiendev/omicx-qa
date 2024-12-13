using Omicx.QA.Enums;

namespace Omicx.QA.Services.Email.Dto;

public class EmailReceiveAttributeDto
{
    public Guid? EmailReceiveId { get; set; }
    
    public Guid? TenantId { get; set; }

    public int? CustomTenantId { get; set; }
    
    public Guid DynamicAttributeId { get; set; }
    
    public required string SystemName { get; set; }
    
    public required string DisplayName { get; set; }
    
    public required DynamicAttributeType Type { get; set; }

    public required string Value { get; set; }
}