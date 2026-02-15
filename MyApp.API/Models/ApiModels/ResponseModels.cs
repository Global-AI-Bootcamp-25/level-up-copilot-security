public class ResponseModel<T>
{
    public T Data { get; set; }
    public string Message { get; set; }
    public bool Success { get; set; }

    public ResponseModel(T data, string message = "", bool success = true)
    {
        Data = data;
        Message = message;
        Success = success;
    }
}

public class ErrorResponseModel
{
    public string Error { get; set; }
    public string Details { get; set; }

    public ErrorResponseModel(string error, string details = "")
    {
        Error = error;
        Details = details;
    }
}