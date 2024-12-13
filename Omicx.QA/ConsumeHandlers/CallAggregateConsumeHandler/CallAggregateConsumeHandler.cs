using Confluent.Kafka;
using CrmCloud.Kafka;
using Omicx.QA.JsonRequests.Call;
using Omicx.QA.Services.Call;

namespace Omicx.QA.ConsumeHandlers.CallAggregateConsumeHandler;

public class CallAggregateConsumeHandler : TransientJsonConsumeHandler<CallEventRequest>
{
    private readonly ILogger<CallAggregateConsumeHandler> _logger;
    private readonly ICallAggregateAppService _callAggregateAppService;
    
    public CallAggregateConsumeHandler(
        ILogger<CallAggregateConsumeHandler> logger,
        ICallAggregateAppService callAggregateAppService
    )
    {
        _logger = logger;
        _callAggregateAppService = callAggregateAppService;
    }
    
    public override string[] Topics => new[] { "CallAggregate" };
    
    protected override async void OnMessageArrived(ConsumeContext<Null, CallEventRequest> context, Null key, CallEventRequest eventRequest)
    {
        await _callAggregateAppService.InsertJobSyncCallAggregate(eventRequest);
        context.CommitOffset();
    }

    protected override void OnError(ConsumeContext<Null, CallEventRequest> context, KafkaException exception)
    {
        _logger.LogError("CallAggregateConsumeHandler ERROR", exception);
    }
}