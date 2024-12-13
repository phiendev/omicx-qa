using Confluent.Kafka;
using CrmCloud.Kafka;
using Omicx.QA.JsonRequests.Email;
using Omicx.QA.Services.Email;

namespace Omicx.QA.ConsumeHandlers.EmailReceiveConsumeHandler;

public class EmailReceiveConsumeHandler : TransientJsonConsumeHandler<EmailEventRequest>
{
    private readonly ILogger<EmailReceiveConsumeHandler> _logger;
    private readonly IEmailReceiveAppService _emailReceiveAppService;
    
    public EmailReceiveConsumeHandler(
        ILogger<EmailReceiveConsumeHandler> logger,
        IEmailReceiveAppService emailReceiveAppService
    )
    {
        _logger = logger;
        _emailReceiveAppService = emailReceiveAppService;
    }
    
    public override string[] Topics => new[] { "EmailReceive" };
    
    protected override async void OnMessageArrived(ConsumeContext<Null, EmailEventRequest> context, Null key, EmailEventRequest eventRequest)
    {
        await _emailReceiveAppService.InsertJobSyncEmailReceive(eventRequest);
        context.CommitOffset();
    }

    protected override void OnError(ConsumeContext<Null, EmailEventRequest> context, KafkaException exception)
    {
        _logger.LogError("EmailReceiveConsumeHandler ERROR", exception);
    }
}