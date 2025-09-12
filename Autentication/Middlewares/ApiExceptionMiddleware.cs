using Autentication.Application.Services.Exceptions;
using Autentication.Core.Entities.Core;

namespace Autentication.Web.Middlewares
{
    // Web/Middlewares/ApiExceptionMiddleware.cs
    // Web/Middlewares/ApiExceptionMiddleware.cs
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
            catch (DomainException ex) // tus excepciones de dominio
            {
                var (status, expose) = MapStatus(ex.ErrorCode);
                _log.LogWarning(ex, "Domain error {Code}", ex.ErrorCode);

                ctx.Response.StatusCode = status;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(new ApiError
                {
                    Code = ex.ErrorCode,
                    Message = expose ? ex.Message : "Operación inválida.",
                    CorrelationId = ctx.TraceIdentifier
                });
            }
            catch (UnauthorizedAccessException)
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(new ApiError { Code = "UNAUTHORIZED", Message = "No autorizado." });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unhandled error");
                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(new ApiError { Code = "INTERNAL_ERROR", Message = "Error interno." });
            }
        }

        private static (int status, bool expose) MapStatus(string code) => code switch
        {
            "USER_DUPLICATE" => (StatusCodes.Status409Conflict, true),
            "WEAK_PASSWORD" => (StatusCodes.Status400BadRequest, true),
            "APP_NOT_FOUND" => (StatusCodes.Status400BadRequest, true),
            "ROLE_NOT_FOUND" => (StatusCodes.Status400BadRequest, true),
            "USER_NOT_FOUND" => (StatusCodes.Status404NotFound, true),
            "USER_LOCKED" => (StatusCodes.Status423Locked, true),
            "INVALID_CREDENTIALS" => (StatusCodes.Status401Unauthorized, false),
            "REFRESH_INVALID" => (StatusCodes.Status401Unauthorized, true),
            _ => (StatusCodes.Status400BadRequest, false)
        };
    }


}
