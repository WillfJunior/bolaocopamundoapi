using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado [{Type}]: {Message}", ex.GetType().Name, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, ex.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, ex.Message),
            InvalidOperationException => (StatusCodes.Status400BadRequest, ex.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, ex.Message),
            DbUpdateException dbEx => (StatusCodes.Status400BadRequest, ExtrairMensagemBanco(dbEx)),
            _ => (StatusCodes.Status500InternalServerError, $"Erro interno: {ex.GetType().Name} - {ex.Message}")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = JsonSerializer.Serialize(new { error = message, statusCode });

        return context.Response.WriteAsync(response);
    }

    private static string ExtrairMensagemBanco(DbUpdateException ex)
    {
        var inner = ex.InnerException?.Message ?? ex.Message;
        if (inner.Contains("UNIQUE") || inner.Contains("duplicate") || inner.Contains("IX_Users_PhoneNumber"))
            return "Telefone já cadastrado.";
        return $"Erro ao salvar no banco: {inner}";
    }
}
