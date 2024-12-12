namespace Omicx.QA.Services.DynamicEntity.Dto;

public class DynamicEntityDto
{
    public Guid Id { get; set; }
    public required string Label { get; set; }
    public ICollection<AttributeGroupDto>? AttributeGroups { get; set; }
    public ICollection<DynamicAttributeDto>? DynamicAttributes { get; set; }
}