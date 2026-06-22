namespace SmartBookAPI.DTOs;

/// <summary>
/// Formato estándar para todas las respuestas de la API
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default
        };
    }
}

/// <summary>
/// Respuesta sin datos (solo mensaje)
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    public static ApiResponse Ok(string message = "Operación exitosa")
    {
        return new ApiResponse { Success = true, Message = message };
    }

    public static ApiResponse Fail(string message)
    {
        return new ApiResponse { Success = false, Message = message };
    }
}
