using Omicx.QA.Services.DynamicEntity.Dto;

namespace Omicx.QA.Services.DynamicEntity;

public interface IDynamicEntityAppService
{
    Task SyncScheme();
    Task<DynamicEntitySchemaDto> CreateSchema(DynamicEntitySchemaDto item);
    Task<DynamicEntitySchemaDto> UpdateSchema(DynamicEntitySchemaDto item);
    Task DeleteSchema(Guid id);
    Task<AttributeGroupDto> CreateAttributeGroup(AttributeGroupDto item);
    Task<AttributeGroupDto> UpdateAttributeGroup(AttributeGroupDto item);
    Task DeleteAttributeGroup(Guid id);
    Task<DynamicAttributeDto> CreateDynamicAttribute(DynamicAttributeDto item);
    Task<DynamicAttributeDto> UpdateDynamicAttribute(DynamicAttributeDto item);
    Task DeleteDynamicAttribute(Guid id);
}