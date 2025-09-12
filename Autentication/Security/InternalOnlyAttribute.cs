using Autentication.Core.Entities.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;
using System.Text;

namespace Autentication.Web.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InternalOnlyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var cfg = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var expected = Environment.GetEnvironmentVariable("INTERNAL_SECRET") ?? cfg["Internal:Secret"];

            if (string.IsNullOrWhiteSpace(expected))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            string? provided = null;

            // 1) Header
            if (context.HttpContext.Request.Headers.TryGetValue("X-Internal-Secret", out var got) && !StringValues.IsNullOrEmpty(got))
                provided = got.ToString();

            // 2) (Opcional) Query en Development
            var env = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();
            if (provided is null && env.IsDevelopment())
                provided = context.HttpContext.Request.Query["internal_secret"];

            if (!SecureEquals(provided, expected))
            {
                context.Result = new JsonResult(new ApiError { Code = "UNAUTHORIZED", Message = "No autorizado." })
                { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }

        private static bool SecureEquals(string? a, string? b)
        {
            if (a is null || b is null) return false;
            var ba = Encoding.UTF8.GetBytes(a);
            var bb = Encoding.UTF8.GetBytes(b);
            return CryptographicOperations.FixedTimeEquals(ba, bb);
        }
    }
}
