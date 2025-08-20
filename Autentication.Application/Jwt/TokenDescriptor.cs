namespace Autentication.Application.Jwt
{
    public sealed record TokenDescriptor(
        string Subject,                 // sub (UserId)
        IEnumerable<string> Roles,      // roles (array)
        string Issuer,                  // iss
        string Audience,                // aud
        string Jti,                     // jti
        TimeSpan Lifetime,              // exp (ahora + Lifetime)
        IDictionary<string, object>? Claims = null  // 👈 NUEVO: claims extra (name, mfa, etc.)
    );
}
