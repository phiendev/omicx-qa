using Omicx.QA.Enums;

namespace Omicx.QA.Services.DynamicEntity.Dto;

public class DynamicAttributeDto
{
    public Guid Id { get; set; }
    
    public Guid? TenantId { get; set; }
    
    public int? CustomTenantId { get; set; }
    
    public Guid? DynamicEntitySchemaId { get; set; }
    
    public string? EntityType { get; set; }

    public Guid? AttributeGroupId { get; set; }
    
    public string? AttributeGroupCode { get; set; }
    
    public required DynamicAttributeType Type { get; set; }
    
    public required string SystemName { get; set; }

    public required string DisplayName { get; set; }

    public string? DesignerOptions { get; set; }
    
    public bool IsActive { get; set; }
    
    public int? Order { get; set; }
}