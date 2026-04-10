namespace Models.dto;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    // Static helpers for efficiency
    public static ServiceResult<T> Ok(T data, string msg = "Success") 
        => new ServiceResult<T> { Success = true, Data = data, Message = msg };

    public static ServiceResult<T> Fail(string msg) 
        => new ServiceResult<T> { Success = false, Message = msg };
}