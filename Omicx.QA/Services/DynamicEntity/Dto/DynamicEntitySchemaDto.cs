namespace Omicx.QA.Services.DynamicEntity.Dto;

public class DynamicEntitySchemaDto
{
    public Guid Id { get; set; }
    
    public Guid? TenantId { get; set; }
    
    public int? CustomTenantId { get; set; }
    
    public required string EntityType { get; set; }
    
    public required string Label { get; set; }
    
    public required string HashedProperties { get; set; }
}