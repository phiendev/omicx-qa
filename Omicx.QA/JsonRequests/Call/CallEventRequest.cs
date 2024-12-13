namespace Omicx.QA.JsonRequests.Call;

public class CallEventRequest
{
    public string? CallId { get; set; }
    public Assignee? Assignee { get; set; }
    public string? RecordingUrl { get; set; }
    public string? Content { get; set; }
}