using AutoMapper;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Elasticsearch.Extensions;
using Omicx.QA.Entities.CallAggregate;
using Omicx.QA.Services.Call.Dto;
using Omicx.QA.Services.Call.Request;

namespace Omicx.QA.Services.Call.Mapper;

public class CallAggregateMapperProfile : Profile
{
    public CallAggregateMapperProfile()
    {
        CreateMap<CallAggregate, CallAggregateDto>().ReverseMap();
        CreateMap<CallAggregateRequest, CallAggregate>();
        CreateMap<CallAggregate, CallAggregateDocument>()
            .DynamicAttributes(t => t.CallAggregateAttributes)
            .ForMember(dest => dest.Keys, opt => opt.Ignore());
        
        CreateMap<CallAggregateAttribute, CallAggregateAttributeDto>().ReverseMap();
    }
}