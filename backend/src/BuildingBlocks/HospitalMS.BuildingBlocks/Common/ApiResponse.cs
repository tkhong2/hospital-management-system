namespace HospitalMS.BuildingBlocks.Common;

public class ApiResponse
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public List<string>? Errors { get; set; }
    public string? TraceId { get; set; }

    public static ApiResponse Ok(object? data = null, string? traceId = null) => new()
    {
        Success = true,
        Data = data,
        TraceId = traceId
    };

    public static ApiResponse Fail(string error, string? traceId = null) => new()
    {
        Success = false,
        Errors = [error],
        TraceId = traceId
    };

    public static ApiResponse Fail(List<string> errors, string? traceId = null) => new()
    {
        Success = false,
        Errors = errors,
        TraceId = traceId
    };
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public string? TraceId { get; set; }

    public static ApiResponse<T> Ok(T data, string? traceId = null) => new()
    {
        Success = true,
        Data = data,
        TraceId = traceId
    };

    public static ApiResponse<T> Fail(string error, string? traceId = null) => new()
    {
        Success = false,
        Errors = [error],
        TraceId = traceId
    };

    public static ApiResponse<T> Fail(List<string> errors, string? traceId = null) => new()
    {
        Success = false,
        Errors = errors,
        TraceId = traceId
    };
}
