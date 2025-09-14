using System.Collections.Generic;

namespace Arsmate.Core.DTOs.Common
{
    /// <summary>
    /// DTO para errores de validación
    /// </summary>
    public class ErrorDto
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public Dictionary<string, List<string>> ValidationErrors { get; set; }

        public ErrorDto()
        {
            ValidationErrors = new Dictionary<string, List<string>>();
        }

        public ErrorDto(string code, string message)
        {
            Code = code;
            Message = message;
            ValidationErrors = new Dictionary<string, List<string>>();
        }
    }
}