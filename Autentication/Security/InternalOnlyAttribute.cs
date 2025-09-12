using Autentication.Core.Entities.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Autentication.Web.Security
{

    public sealed class InternalOnlyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var cfg = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var expected = cfg["Internal:Secret"];
            if (string.IsNullOrEmpty(expected))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("X-Internal-Secret", out var got) ||
                !string.Equals(got.ToString(), expected, StringComparison.Ordinal))
            {
                context.Result = new JsonResult(new ApiError { Code = "UNAUTHORIZED", Message = "No autorizado." })
                { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
