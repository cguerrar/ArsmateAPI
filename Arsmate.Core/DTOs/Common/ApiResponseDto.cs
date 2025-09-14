using System;

namespace Arsmate.Core.DTOs.Common
{
    /// <summary>
    /// DTO estándar para respuestas de API
    /// </summary>
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string TraceId { get; set; }

        public ApiResponseDto()
        {
            Timestamp = DateTime.UtcNow;
            TraceId = Guid.NewGuid().ToString();
        }

        public static ApiResponseDto<T> SuccessResponse(T data, string message = "Operation successful")
        {
            return new ApiResponseDto<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponseDto<T> ErrorResponse(string message)
        {
            return new ApiResponseDto<T>
            {
                Success = false,
                Message = message,
                Data = default(T)
            };
        }
    }
}
