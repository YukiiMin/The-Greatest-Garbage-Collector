namespace GarbageCollection.Common.DTOs
{
    public class ApiResponse<T>
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public ApiError? Error { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "success") => new()
        {
            Status = "success",
            Message = message,
            Data = data,
            Error = null
        };

        // Alias dùng bởi AuthController (message trước, data sau)
        public static ApiResponse<T> Success(string message, T data) => Ok(data, message);

        public static ApiResponse<T> Fail(string message, string code = "ERROR", string? description = null) => new()
        {
            Status = "failed",
            Message = message,
            Data = default,
            Error = new ApiError { Code = code, Description = description ?? message }
        };
    }

    public class ApiError
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
