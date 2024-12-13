namespace Omicx.QA.JsonRequests.Email;

public class EmailEventRequest
{
    public string? EmailId { get; set; }
    public Assignee? Assignee { get; set; }
    public string? RecordingUrl { get; set; }
    public string? Content { get; set; }
}