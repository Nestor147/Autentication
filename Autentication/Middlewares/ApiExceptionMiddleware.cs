using Autentication.Application.Services.Exceptions; // si usas DomainException ahí
using Autentication.Core.Entities.Core;              // ApiError
using Microsoft.EntityFrameworkCore;

namespace Autentication.Web.Middlewares
{
    public class ApiExceptionMiddleware : IMiddleware
    {
        private readonly ILogger<ApiExceptionMiddleware> _log;
        public ApiExceptionMiddleware(ILogger<ApiExceptionMiddleware> log) => _log = log;

        public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
        {
            try
            {
                await next(ctx);
            }
            catch (DomainException ex)
            {
                var (status, expose) = MapStatus(ex.ErrorCode);
                _log.LogWarning(ex, "Domain error {Code}", ex.ErrorCode);
                await WriteProblemAsync(ctx, status, new ApiError
                {
                    Code = ex.ErrorCode,
                    Message = expose ? ex.Message : "Operación inválida.",
                    CorrelationId = ctx.TraceIdentifier
                });
            }
            catch (DbUpdateException ex) when (IsUniqueConstraint(ex))
            {
                _log.LogWarning(ex, "Unique constraint violation");
                await WriteProblemAsync(ctx, StatusCodes.Status409Conflict, new ApiError
                {
                    Code = "CONFLICT",
                    Message = "Conflicto con un índice único.",
                    CorrelationId = ctx.TraceIdentifier
                });
            }
            catch (BadHttpRequestException ex)
            {
                _log.LogInformation(ex, "Bad HTTP request");
                await WriteProblemAsync(ctx, StatusCodes.Status400BadRequest, new ApiError
                {
                    Code = "BAD_REQUEST",
                    Message = ex.Message,
                    CorrelationId = ctx.TraceIdentifier
                });
            }
            catch (OperationCanceledException ex)
            {
                _log.LogInformation(ex, "Request canceled by client");
                // muchos proxies usan 499 para client closed request; usa 400 si prefieres estándar puro
                await WriteProblemAsync(ctx, 499, new ApiError
                {
                    Code = "CLIENT_CANCELED",
                    Message = "La solicitud fue cancelada por el cliente.",
                    CorrelationId = ctx.TraceIdentifier
                });
            }
            catch (UnauthorizedAccessException)
            {
                await WriteProblemAsync(ctx, StatusCodes.Status401Unauthorized, new ApiError
                {
                    Code = "UNAUTHORIZED",
                    Message = "No autorizado.",
                    CorrelationId = ctx.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unhandled error");
                await WriteProblemAsync(ctx, StatusCodes.Status500InternalServerError, new ApiError
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Error interno.",
                    CorrelationId = ctx.TraceIdentifier
                });
            }
        }

        private static (int status, bool expose) MapStatus(string code) => code switch
        {
            "USER_DUPLICATE" => (StatusCodes.Status409Conflict, true),
            "WEAK_PASSWORD" => (StatusCodes.Status400BadRequest, true),
            "APP_NOT_FOUND" => (StatusCodes.Status400BadRequest, true),
            "ROLE_NOT_FOUND" => (StatusCodes.Status400BadRequest, true),
            "ROLE_ALREADY_ASSIGNED" => (StatusCodes.Status409Conflict, true),
            "GENERAL_USER_ALREADY_LINKED" => (StatusCodes.Status409Conflict, true),
            "USER_NOT_FOUND" => (StatusCodes.Status404NotFound, true),
            "USER_LOCKED" => (StatusCodes.Status423Locked, true),
            "INVALID_CREDENTIALS" => (StatusCodes.Status401Unauthorized, false),
            "REFRESH_INVALID" => (StatusCodes.Status401Unauthorized, true),
            _ => (StatusCodes.Status400BadRequest, false)
        };

        private static bool IsUniqueConstraint(DbUpdateException ex)
        {
            // SQL Server: 2601 / 2627; si quieres, inspecciona el nombre del índice en ex.InnerException?.Message
            var msg = ex.InnerException?.Message ?? ex.Message;
            return msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("IX_", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("UX_", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("2627")
                || msg.Contains("2601");
        }

        private static async Task WriteProblemAsync(HttpContext ctx, int status, ApiError payload)
        {
            if (ctx.Response.HasStarted) return;

            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/json";
            // agrega el id también como header para correlación en logs/APM
            ctx.Response.Headers["X-Request-Id"] = ctx.TraceIdentifier;
            await ctx.Response.WriteAsJsonAsync(payload);
        }
    }
}
