using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Application.Jwt
{
    public static class JwtAuthExtensions
    {
        /// <summary>
        /// Valida JWT usando OpenID Configuration o JWKS (recomendado para key rollover).
        /// openIdConfigOrJwksUrl: puede ser ".../.well-known/openid-configuration" o directamente ".../jwks.json".
        /// </summary>
        public static IServiceCollection AddJwtFromJwks(
            this IServiceCollection services, string issuer, string audience, string openIdConfigOrJwksUrl)
        {
            IConfigurationManager<OpenIdConnectConfiguration> configManager =
                openIdConfigOrJwksUrl.EndsWith("jwks.json", StringComparison.OrdinalIgnoreCase)
                ? new StaticJwksConfigurationManager(openIdConfigOrJwksUrl)
                : new ConfigurationManager<OpenIdConnectConfiguration>(
                      openIdConfigOrJwksUrl, new OpenIdConnectConfigurationRetriever());

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidateIssuerSigningKey = true
                };
                o.ConfigurationManager = configManager;
            });

            return services;
        }

        /// <summary>
        /// Valida JWT con clave pública RSA pinneada (sin llamadas de red).
        /// </summary>
        public static IServiceCollection AddJwtWithPinnedRsa(
            this IServiceCollection services, string issuer, string audience, string publicKeyPem)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem.AsSpan());
            var key = new RsaSecurityKey(rsa);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key
                };
            });

            return services;
        }

        // Cargador de JWKS estático (si apuntas directo a jwks.json)
        private sealed class StaticJwksConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
        {
            private readonly string _jwksUrl;
            private OpenIdConnectConfiguration? _cached;

            public StaticJwksConfigurationManager(string jwksUrl) => _jwksUrl = jwksUrl;

            public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
            {
                if (_cached is not null) return _cached;
                using var http = new HttpClient();
                var json = await http.GetStringAsync(_jwksUrl, cancel);
                var jwks = new JsonWebKeySet(json);
                var cfg = new OpenIdConnectConfiguration();
                foreach (var k in jwks.Keys) cfg.SigningKeys.Add(k);
                _cached = cfg;
                return _cached;
            }

            public void RequestRefresh() => _cached = null;
        }
    }
}
