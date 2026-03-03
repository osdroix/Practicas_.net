namespace TaskManager.Web.Models
{
    // ----------------------------------------------------------------------------------
    // Date: 2026-02-11
    // Author: [Usuario]
    // Description: Clase genérica para estandarizar las respuestas de la capa de servicios.
    // Evita el uso de excepciones para el flujo de control normal (como validaciones fallidas).
    // Success: indica si la operación fue exitosa.
    // Message: mensaje de error o éxito para mostrar al usuario.
    // Data: (En la versión genérica) los datos devueltos por la operación.
    // ----------------------------------------------------------------------------------
    public class ServiceResult
    {
        public bool Success { get; protected set; }
        public string Message { get; protected set; }
        public int? StatusCode { get; protected set; }
        public string Detail { get; protected set; }

        public static ServiceResult Ok(string message = null)
        {
            return new ServiceResult { Success = true, Message = message };
        }

        public static ServiceResult Fail(string message)
        {
            return new ServiceResult { Success = false, Message = message };
        }

        public static ServiceResult Fail(string message, int? statusCode, string detail = null)
        {
            return new ServiceResult { Success = false, Message = message, StatusCode = statusCode, Detail = detail };
        }
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T Data { get; private set; }

        public static ServiceResult<T> Ok(T data, string message = null)
        {
            return new ServiceResult<T> { Success = true, Data = data, Message = message };
        }

        public new static ServiceResult<T> Fail(string message)
        {
            return new ServiceResult<T> { Success = false, Message = message };
        }

        public static ServiceResult<T> Fail(string message, int? statusCode, string detail = null)
        {
            return new ServiceResult<T> { Success = false, Message = message, StatusCode = statusCode, Detail = detail };
        }
    }
}
