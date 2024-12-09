using Confluent.Kafka;
using CrmCloud.Kafka;
using Omicx.QA.JsonRequests.Todo;

namespace Omicx.QA.ConsumeHandlers.TodoConsumeHandler;

public class TodoConsumeHandler : TransientJsonConsumeHandler<TodoRequest>
{
    private readonly Castle.Core.Logging.ILogger _logger;
    
    public TodoConsumeHandler(
        Castle.Core.Logging.ILogger logger
    )
    {
        _logger = logger;
    }
    
    public override string[] Topics => new[] { "Omicx.Ticket.CreatedEvent" };
    
    protected override async void OnMessageArrived(ConsumeContext<Null, TodoRequest> context, Null key, TodoRequest request)
    {
        context.CommitOffset();
    }

    protected override void OnError(ConsumeContext<Null, TodoRequest> context, KafkaException exception)
    {
        _logger.Error("TodoConsumeHandler ERROR", exception);
    }
}