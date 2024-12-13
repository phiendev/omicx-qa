using Omicx.QA.Common.Responses;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Elasticsearch.Requests;
using Omicx.QA.JsonRequests.Call;
using Omicx.QA.Services.Call.Dto;
using Omicx.QA.Services.Call.Request;

namespace Omicx.QA.Services.Call;

public interface ICallAggregateAppService
{
    Task<CallAggregateDto> GetCallAggregate(Guid id);
    Task<DocumentResponse<IEnumerable<CallAggregateDocument>>> GetCallAggregates(FilterDocumentRequest input);
    Task InsertJobSyncCallAggregate(CallEventRequest request);
    Task<CallAggregateDto> CreateCallAggregate(CallAggregateRequest item);
    Task<CallAggregateDto> UpdateCallAggregate(CallAggregateRequest item);
    Task DeleteCallAggregate(Guid id);
}