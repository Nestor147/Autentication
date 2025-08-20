using Autentication.Application.Jwt;

namespace Autentication.Application.Interfaces.Jwt
{
    public interface IJwtIssuer
    {
        string CreateAccessToken(TokenDescriptor descriptor);
    }
}
