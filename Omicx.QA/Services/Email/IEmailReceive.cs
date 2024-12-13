using Omicx.QA.Common.Responses;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Elasticsearch.Requests;
using Omicx.QA.Services.Email.Dto;
using Omicx.QA.Services.Email.Request;

namespace Omicx.QA.Services.Email;

public interface IEmailReceive
{
    Task<EmailReceiveDto> GetEmailReceive(Guid id);
    Task<DocumentResponse<IEnumerable<EmailReceiveDocument>>> GetEmailReceives(FilterDocumentRequest input);
    Task<EmailReceiveDto> CreateEmailReceive(EmailReceiveRequest item);
    Task<EmailReceiveDto> UpdateEmailReceive(EmailReceiveRequest item);
    Task DeleteEmailReceive(Guid id);
}