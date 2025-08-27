////using Autentication.Application.Interfaces.Jwt;
////using Microsoft.IdentityModel.Tokens;
////using System;
////using System.Collections.Generic;
////using System.IdentityModel.Tokens.Jwt;
////using System.Linq;
////using System.Security.Claims;
////using System.Security.Cryptography;
////using System.Text;
////using System.Threading.Tasks;

////namespace Autentication.Application.Jwt
////{
////    public sealed class RsaJwtIssuer : IJwtIssuer, IDisposable
////    {
////        private readonly RSA _rsa;
////        private readonly RsaSecurityKey _key;
////        private readonly JwtSecurityTokenHandler _handler = new();

////        // privateKeyPem: contenido PEM de PKCS#8 ó PKCS#1
////        // keyId: opcional pero recomendable para key rollover
////        public RsaJwtIssuer(string privateKeyPem, string? keyId = null)
////        {
////            _rsa = RSA.Create();
////            _rsa.ImportFromPem(privateKeyPem.AsSpan());
////            _key = new RsaSecurityKey(_rsa) { KeyId = keyId ?? Guid.NewGuid().ToString("N") };
////        }

////        public string CreateAccessToken(TokenDescriptor d)
////        {
////            var now = DateTime.UtcNow;

////            var claims = new List<Claim>
////        {
////            new(JwtRegisteredClaimNames.Sub, d.Subject),
////            new(JwtRegisteredClaimNames.Jti, d.Jti),
////            new(JwtRegisteredClaimNames.Iss, d.Issuer),
////            new(JwtRegisteredClaimNames.Aud, d.Audience),
////            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
////        };

////            if (d.Roles is not null)
////                claims.AddRange(d.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

////            var token = new JwtSecurityToken(
////                issuer: d.Issuer,
////                audience: d.Audience,
////                claims: claims,
////                notBefore: now,
////                expires: now.Add(d.Lifetime),
////                signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.RsaSha256));

////            return _handler.WriteToken(token);
////        }

////        public void Dispose()
////        {
////            _rsa.Dispose();
////            GC.SuppressFinalize(this);
////        }
////    }

////    file static class DateTimeExtensions
////    {
////        public static long ToUnixTimeSeconds(this DateTime dt) =>
////            (long)Math.Floor((dt - DateTime.UnixEpoch).TotalSeconds);
////    }
////}


//using Autentication.Application.Interfaces.Jwt;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using Microsoft.IdentityModel.Tokens;

//namespace Autentication.Application.Jwt
//{
//    public sealed class RsaJwtIssuer : IJwtIssuer, IDisposable
//    {
//        private readonly RSA _rsa;
//        private readonly RsaSecurityKey _key;
//        private readonly SigningCredentials _creds;
//        private static readonly JwtSecurityTokenHandler _handler = new(); // thread-safe
//        private readonly string _kid;

//        /// <summary>
//        /// Crea un emisor RS256.
//        /// </summary>
//        /// <param name="privateKeyPem">PEM PKCS#8 o PKCS#1</param>
//        /// <param name="keyId">ID de la clave (kid) para JWKS/rollover</param>
//        public RsaJwtIssuer(string privateKeyPem, string? keyId = null)
//        {
//            if (string.IsNullOrWhiteSpace(privateKeyPem))
//                throw new ArgumentException("Private key PEM is required.", nameof(privateKeyPem));

//            _rsa = RSA.Create();
//            _rsa.ImportFromPem(privateKeyPem.AsSpan());

//            _key = new RsaSecurityKey(_rsa);
//            _kid = string.IsNullOrWhiteSpace(keyId) ? Guid.NewGuid().ToString("N") : keyId;
//            _key.KeyId = _kid;

//            _creds = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256);
//        }

//        public string CreateAccessToken(TokenDescriptor d)
//        {
//            if (d is null) throw new ArgumentNullException(nameof(d));
//            if (string.IsNullOrWhiteSpace(d.Subject)) throw new ArgumentException("Subject is required.", nameof(d.Subject));
//            if (string.IsNullOrWhiteSpace(d.Issuer)) throw new ArgumentException("Issuer is required.", nameof(d.Issuer));
//            if (string.IsNullOrWhiteSpace(d.Audience)) throw new ArgumentException("Audience is required.", nameof(d.Audience));
//            if (string.IsNullOrWhiteSpace(d.Jti)) throw new ArgumentException("Jti is required.", nameof(d.Jti));
//            if (d.Lifetime <= TimeSpan.Zero) throw new ArgumentException("Lifetime must be positive.", nameof(d.Lifetime));

//            var now = DateTime.UtcNow;
//            var iat = ToUnix(now);
//            var nbf = iat;
//            var exp = ToUnix(now.Add(d.Lifetime));

//            // Claims estándar (como NumericDate)
//            var payload = new Dictionary<string, object>
//            {
//                [JwtRegisteredClaimNames.Sub] = d.Subject,
//                [JwtRegisteredClaimNames.Jti] = d.Jti,
//                [JwtRegisteredClaimNames.Iss] = d.Issuer,
//                [JwtRegisteredClaimNames.Aud] = d.Audience,
//                [JwtRegisteredClaimNames.Iat] = iat,
//                ["nbf"] = nbf,
//                ["exp"] = exp
//            };

//            // Claims extra (se sobreescriben si repiten clave)
//            if (d.Claims is not null)
//            {
//                foreach (var kv in d.Claims)
//                    payload[kv.Key] = kv.Value;
//            }

//            // Roles: como array "roles" y también como claims "role" para compatibilidad
//            if (d.Roles is not null)
//            {
//                var rolesArr = d.Roles.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToArray();
//                if (rolesArr.Length > 0)
//                    payload["roles"] = rolesArr;
//            }

//            var sdesc = new SecurityTokenDescriptor
//            {
//                Issuer = d.Issuer,
//                Audience = d.Audience,
//                Claims = payload,
//                SigningCredentials = _creds
//            };

//            var token = _handler.CreateJwtSecurityToken(sdesc);
//            token.Header["kid"] = _kid; // importante para JWKS

//            // (Opcional) duplicar roles como claims "role" para consumidores legacy
//            if (d.Roles is not null)
//            {
//                foreach (var r in d.Roles)
//                    token.Payload.AddClaim(new Claim(ClaimTypes.Role, r));
//            }

//            return _handler.WriteToken(token);
//        }

//        /// <summary>
//        /// Exporta la clave pública como JWK (útil para /.well-known/jwks.json).
//        /// </summary>
//        public string ExportJwkJson()
//        {
//            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(_key);
//            jwk.KeyId = _kid;
//            return System.Text.Json.JsonSerializer.Serialize(new { keys = new[] { jwk } });
//        }

//        public void Dispose()
//        {
//            _rsa.Dispose();
//            GC.SuppressFinalize(this);
//        }

//        private static long ToUnix(DateTime dt) => new DateTimeOffset(dt).ToUnixTimeSeconds();
//    }
//}


using Autentication.Application.Interfaces.Jwt;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Autentication.Application.Jwt
{
    public sealed class RsaJwtIssuer : IJwtIssuer, IDisposable
    {
        private readonly RSA _rsa;
        private readonly RsaSecurityKey _key;
        private readonly SigningCredentials _creds;
        private static readonly JwtSecurityTokenHandler _handler = new(); // thread-safe
        private readonly string _kid;

        public RsaJwtIssuer(string privateKeyPem, string? keyId = null)
        {
            if (string.IsNullOrWhiteSpace(privateKeyPem))
                throw new ArgumentException("Private key PEM is required.", nameof(privateKeyPem));

            _rsa = RSA.Create();
            _rsa.ImportFromPem(privateKeyPem.AsSpan());

            _key = new RsaSecurityKey(_rsa);
            _kid = string.IsNullOrWhiteSpace(keyId) ? Guid.NewGuid().ToString("N") : keyId;
            _key.KeyId = _kid;

            _creds = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256);
        }

        public string CreateAccessToken(TokenDescriptor d)
        {
            if (d is null) throw new ArgumentNullException(nameof(d));
            if (string.IsNullOrWhiteSpace(d.Subject)) throw new ArgumentException("Subject is required.", nameof(d.Subject));
            if (string.IsNullOrWhiteSpace(d.Issuer)) throw new ArgumentException("Issuer is required.", nameof(d.Issuer));
            if (string.IsNullOrWhiteSpace(d.Audience)) throw new ArgumentException("Audience is required.", nameof(d.Audience));
            if (string.IsNullOrWhiteSpace(d.Jti)) throw new ArgumentException("Jti is required.", nameof(d.Jti));
            if (d.Lifetime <= TimeSpan.Zero) throw new ArgumentException("Lifetime must be positive.", nameof(d.Lifetime));

            var now = DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, d.Subject),
                new(JwtRegisteredClaimNames.Jti, d.Jti),
                new(JwtRegisteredClaimNames.Iat, ToUnix(now).ToString(), ClaimValueTypes.Integer64),
            };

            // 👇 agrega roles planos como ClaimTypes.Role si vienen
            if (d.Roles is not null)
                claims.AddRange(d.Roles.Where(r => !string.IsNullOrWhiteSpace(r))
                                       .Distinct()
                                       .Select(r => new Claim(ClaimTypes.Role, r)));

            // 👇 agrega claims adicionales que tú construiste en AuthService (incluye app_roles/apps en JSON)
            if (d.ExtraClaims is not null)
                claims.AddRange(d.ExtraClaims);

            // (opcional) si además pasas d.Claims (diccionario con escalares), los convertimos a Claim
            if (d.Claims is not null)
            {
                foreach (var kv in d.Claims)
                {
                    if (kv.Value is string s)
                        claims.Add(new Claim(kv.Key, s));
                    else
                        claims.Add(new Claim(kv.Key, kv.Value?.ToString() ?? string.Empty));
                }
            }

            var token = new JwtSecurityToken(
                issuer: d.Issuer,
                audience: d.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(d.Lifetime),
                signingCredentials: _creds
            );

            token.Header["kid"] = _kid;
            return _handler.WriteToken(token);
        }

        public string ExportJwkJson()
        {
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(_key);
            jwk.KeyId = _kid;
            return System.Text.Json.JsonSerializer.Serialize(new { keys = new[] { jwk } });
        }

        public void Dispose()
        {
            _rsa.Dispose();
            GC.SuppressFinalize(this);
        }

        private static long ToUnix(DateTime dt) => new DateTimeOffset(dt).ToUnixTimeSeconds();
    }
}
