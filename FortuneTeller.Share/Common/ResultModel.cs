namespace FortuneTeller.Share.Common;

public class ResultModel<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ResultModel<T> Success(T data, string? message = null)
        => new() { IsSuccess = true, Data = data, Message = message };

    public static ResultModel<T> Failure(string message)
        => new() { IsSuccess = false, Message = message };
}

public class ResultModel
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ResultModel Success(string? message = null)
        => new() { IsSuccess = true, Message = message };

    public static ResultModel Failure(string message)
        => new() { IsSuccess = false, Message = message };
}
