namespace Omicx.QA.Common.Responses;

public class BaseResponse
{
    public bool Status { get; set; }
    public string? Message { get; set; }
    
    public BaseResponse(bool Status, string? Message)
    {
        this.Status = Status;
        this.Message = Message;
    }
}

public class DocumentResponse<T> : BaseResponse
{
    public T? Data { get; set; }
    public long? RecordTotal { get; set; }
    public object? Schema { get; set; }
    
    public DocumentResponse(bool Status, string? Message, T? Data)
        : base(Status, Message)
    {
        this.Data = Data;
        this.RecordTotal = 0;
        this.Schema = null;
    }

    public DocumentResponse(bool Status, string? Message, T? Data, long? RecordTotal)
        : base(Status, Message)
    {
        this.Data = Data;
        this.RecordTotal = RecordTotal;
        this.Schema = null;
    }
    
    public DocumentResponse(bool Status, string? Message, T? Data, long RecordTotal, object? Schema)
        : base(Status, Message)
    {
        this.Data = Data;
        this.RecordTotal = RecordTotal;
        this.Schema = Schema;
    }
}