using System.Security.Claims;

namespace Autentication.Application.Jwt
{
    public sealed record TokenDescriptor(
        string Subject,
        IEnumerable<string>? Roles,
        string Issuer,
        string Audience,
        string Jti,
        TimeSpan Lifetime,
        IDictionary<string, object>? Claims = null,
        IEnumerable<Claim>? ExtraClaims = null // 👈 importante
    );

}
