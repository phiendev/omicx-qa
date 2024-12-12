using AutoMapper;
using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Services.DynamicEntity.Dto;

namespace Omicx.QA.Services.DynamicEntity.Mapper;

public class DynamicEntityMapperProfile : Profile
{
    public DynamicEntityMapperProfile()
    {
        CreateMap<DynamicEntitySchema, DynamicEntitySchemaDto>().ReverseMap();
        CreateMap<AttributeGroup, AttributeGroupDto>().ReverseMap();
        CreateMap<DynamicAttribute, DynamicAttributeDto>().ReverseMap();
        CreateMap<DynamicEntitySchema, DynamicEntityDto>();
        
        CreateMap<DynamicEntitySchema, DynamicEntitySchemaDocument>()
            .ForMember(dest => dest.Keys, opt => opt.Ignore());
        CreateMap<AttributeGroup, AttributeGroupDocument>();
        CreateMap<DynamicAttribute, DynamicAttributeDocument>();
    }
}