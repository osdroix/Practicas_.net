namespace TaskManager.DTOs
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        public static ServiceResult<T> Ok(T data) => new ServiceResult<T> { Success = true, Data = data, StatusCode = 200 };
        public static ServiceResult<T> Created(T data) => new ServiceResult<T> { Success = true, Data = data, StatusCode = 201 };
        public static ServiceResult<T> NoContent() => new ServiceResult<T> { Success = true, StatusCode = 204 };
        public static ServiceResult<T> Failure(string message, int statusCode = 400) => new ServiceResult<T> { Success = false, ErrorMessage = message, StatusCode = statusCode };
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        public static ServiceResult Ok() => new ServiceResult { Success = true, StatusCode = 200 };
        public static ServiceResult NoContent() => new ServiceResult { Success = true, StatusCode = 204 };
        public static ServiceResult Failure(string message, int statusCode = 400) => new ServiceResult { Success = false, ErrorMessage = message, StatusCode = statusCode };
    }
}
