using AutoMapper;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Elasticsearch.Extensions;
using Omicx.QA.Entities.EmailReceive;
using Omicx.QA.Services.Email.Dto;
using Omicx.QA.Services.Email.Request;

namespace Omicx.QA.Services.Email.Mapper;

public class EmailReceiveMapperProfile: Profile
{
    public EmailReceiveMapperProfile()
    {
        CreateMap<EmailReceive, EmailReceiveDto>().ReverseMap();
        CreateMap<EmailReceiveRequest, EmailReceive>();
        CreateMap<EmailReceive, EmailReceiveDocument>()
            .DynamicAttributes(t => t.EmailReceiveAttributes)
            .ForMember(dest => dest.Keys, opt => opt.Ignore());
        
        CreateMap<EmailReceiveAttribute, EmailReceiveAttributeDto>().ReverseMap();
    }
}