namespace Omicx.QA.Services.DynamicEntity.Dto;

public class AttributeGroupDto
{
    public Guid Id { get; set; }
    
    public Guid? TenantId { get; set; }
    
    public int? CustomTenantId { get; set; }
    
    public string? AttributeGroupName { get; set; }
    
    public Guid? DynamicEntitySchemaId { get; set; }
}