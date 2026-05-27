namespace SindiOps.API.Helpers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<ApiError>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, List<ApiError>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

public class ApiError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class PaginatedResponse<T>
{
    public bool Success { get; set; } = true;
    public List<T> Data { get; set; } = [];
    public string? NextCursor { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
}
