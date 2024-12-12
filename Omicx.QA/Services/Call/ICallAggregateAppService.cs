using Omicx.QA.Services.Call.Dto;
using Omicx.QA.Services.Call.Request;

namespace Omicx.QA.Services.Call;

public interface ICallAggregateAppService
{
    Task<CallAggregateDto> CreateCallAggregate(CallAggregateRequest item);
    Task<CallAggregateDto> UpdateCallAggregate(CallAggregateRequest item);
    Task DeleteCallAggregate(Guid id);
}