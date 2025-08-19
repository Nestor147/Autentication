using Autentication.Application.DTOs;
using Autentication.Application.Interfaces;
using Autentication.Core.Entities.Autorizacion;
using Autentication.Core.Interfaces.Core;
using Autentication.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyCompany.Security.Jwt;
using MyCompany.Security.Password;
using System.Security.Cryptography;

public sealed class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtIssuer _jwt;
    private readonly IPasswordHasher _hasher;
    private readonly IConfiguration _cfg;
    private readonly IHttpContextAccessor _http;

    public AuthService(
        IUnitOfWork uow,
        IJwtIssuer jwt,
        IPasswordHasher hasher,
        IConfiguration cfg,
        IHttpContextAccessor http
        )
    {
        _uow = uow;
        _jwt = jwt;
        _hasher = hasher;
        _cfg = cfg;
        _http = http;
    }

    public async Task<TokenPair> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        // 1) Usuario
        var user = await _uow.UsuarioSistemaRepository
            .FirstOrDefaultAsync(u => u.Username == req.Username && u.EstadoRegistro == 1, ct);

        if (user is null) { await RegistrarIntento(req.Username, null, false, "Usuario no existe", ct); throw new UnauthorizedAccessException(); }
        if (user.Locked) { await RegistrarIntento(req.Username, user.Id, false, "Usuario bloqueado", ct); throw new UnauthorizedAccessException(); }

        // 2) Password
        if (!_hasher.Verify(user.Password, req.Password))
        {
            await RegistrarIntento(req.Username, user.Id, false, "Password inválido", ct);
            throw new UnauthorizedAccessException();
        }

        // 3) Roles  (JOIN: RolesUsuarios -> Rol)
        var roleNames =
            await _uow.RolUsuarioRepository.Query()
                .Where(ru => ru.IdUsuarioSistema == user.Id && ru.EstadoRegistro == 1)
                .Join(_uow.RolRepository.Query().Where(r => r.EstadoRegistro == 1),
                      ru => ru.IdRol, r => r.Id,
                      (ru, r) => r.Nombre)
                .ToListAsync(ct);

        // 4) Access JWT
        var jti = Guid.NewGuid().ToString("N");
        var access = _jwt.CreateAccessToken(new TokenDescriptor(
            Subject: user.Id.ToString(),
            Roles: roleNames,
            Issuer: _cfg["Auth:Issuer"]!,
            Audience: _cfg["Auth:Audience"]!,
            Jti: jti,
            Lifetime: TimeSpan.FromMinutes(10)
        ));

        // 5) Refresh opaco (guardar HASH)
        var opaque = RefreshTokenHasher.GenerateOpaque();
        var hash = RefreshTokenHasher.Hash(opaque);

        await _uow.RefreshTokenRepository.InsertAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            IdUsuarioSistema = user.Id,
            TokenHash = hash,
            FechaExpiracion = DateTime.UtcNow.AddDays(15),
            IP = GetClientIp(),
            UserAgent = GetUserAgent()
        });

        await _uow.SaveChangesAsync(); // <- NECESARIO
        await RegistrarIntento(req.Username, user.Id, true, "Login ok", ct);

        return new TokenPair(access, opaque);
    }

    public async Task<TokenPair> RefreshAsync(RefreshRequest req, CancellationToken ct)
    {
        var hash = RefreshTokenHasher.Hash(req.RefreshToken);
        var token = await _uow.RefreshTokenRepository
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.EstadoRegistro == 1, ct);

        if (token is null || token.Usado || token.Revocado || token.FechaExpiracion <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh inválido");

        var user = await _uow.UsuarioSistemaRepository
            .FirstOrDefaultAsync(u => u.Id == token.IdUsuarioSistema, ct);

        // Roles
        var roleNames =
            await _uow.RolUsuarioRepository.Query()
                .Where(ru => ru.IdUsuarioSistema == user.Id && ru.EstadoRegistro == 1)
                .Join(_uow.RolRepository.Query().Where(r => r.EstadoRegistro == 1),
                      ru => ru.IdRol, r => r.Id,
                      (ru, r) => r.Nombre)
                .ToListAsync(ct);

        // Rotar el refresh actual
        token.Usado = true;
        token.Revocado = true;

        // Nuevo access + refresh
        var access = _jwt.CreateAccessToken(new TokenDescriptor(
            Subject: user.Id.ToString(),
            Roles: roleNames,
            Issuer: _cfg["Auth:Issuer"]!,
            Audience: _cfg["Auth:Audience"]!,
            Jti: Guid.NewGuid().ToString("N"),
            Lifetime: TimeSpan.FromMinutes(10)
        ));

        var newOpaque = RefreshTokenHasher.GenerateOpaque();
        var newHash = RefreshTokenHasher.Hash(newOpaque);

        await _uow.RefreshTokenRepository.InsertAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            IdUsuarioSistema = user.Id,
            TokenHash = newHash,
            FechaExpiracion = DateTime.UtcNow.AddDays(15),
            IP = GetClientIp(),
            UserAgent = GetUserAgent()
        });

        await _uow.SaveChangesAsync();
        return new TokenPair(access, newOpaque);
    }

    public async Task LogoutAsync(RefreshRequest req, CancellationToken ct)
    {
        var hash = RefreshTokenHasher.Hash(req.RefreshToken);
        var token = await _uow.RefreshTokenRepository.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (token is null) return;

        token.Revocado = true;
        token.Usado = true;
        await _uow.SaveChangesAsync();
    }

    //public string GetJwks()
    //{
    //    // En prod: lee PUBLICA; aquí derivamos de private.pem para simplificar.
    //    var privatePemPath = Path.Combine(AppContext.BaseDirectory, "keys", "private.pem");
    //    var privatePem = File.ReadAllText(privatePemPath);
    //    using var rsa = RSA.Create(); rsa.ImportFromPem(privatePem);
    //    var key = new Microsoft.IdentityModel.Tokens.RsaSecurityKey(rsa) { KeyId = "k1" };
    //    var jwk = Microsoft.IdentityModel.Tokens.JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
    //    return System.Text.Json.JsonSerializer.Serialize(new { keys = new[] { jwk } });
    //}

    public string GetJwks()
    {
        var publicPemPath = Path.Combine(AppContext.BaseDirectory, "keys", "public.pem");
        var publicPem = File.ReadAllText(publicPemPath);

        using var rsa = RSA.Create(); rsa.ImportFromPem(publicPem);
        var key = new Microsoft.IdentityModel.Tokens.RsaSecurityKey(rsa) { KeyId = "k1" }; // o lee el kid de config
        var jwk = Microsoft.IdentityModel.Tokens.JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
        return System.Text.Json.JsonSerializer.Serialize(new { keys = new[] { jwk } });
    }


    private string? GetClientIp() => _http.HttpContext?.Connection.RemoteIpAddress?.ToString();
    private string? GetUserAgent() => _http.HttpContext?.Request.Headers.UserAgent.ToString();

    private async Task RegistrarIntento(string username, int? idUsuario, bool ok, string msg, CancellationToken ct)
    {
        await _uow.IntentoFallidoLoginRepository.InsertAsync(new IntentoFallidoLogin
        {
            IdUsuarioSistema = idUsuario ?? 0,
            Username = username,
            IP = GetClientIp(),
            UserAgent = GetUserAgent(),
            Exitoso = ok
        });

        await _uow.AuditoriaLoginRepository.InsertAsync(new AuditoriaLogin
        {
            IdUsuarioSistema = idUsuario,
            Username = username,
            IP = GetClientIp(),
            UserAgent = GetUserAgent(),
            Exitoso = ok,
            Mensaje = msg
        });

        await _uow.SaveChangesAsync();
    }


    public async Task<RegisterResponse> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        // 1) Validar duplicados
        var exists = await _uow.UsuarioSistemaRepository
            .FirstOrDefaultAsync(u => u.Username == req.Username && u.EstadoRegistro == 1, ct);
        if (exists is not null)
            throw new InvalidOperationException("El username ya está registrado.");

        // 2) Hash de password
        var hash = _hasher.Hash(req.Password);

        // 3) Construir entidad
        var user = new UsuarioSistema
        {
            IdUsuarioGeneral = req.IdUsuarioGeneral ?? 0, // si manejas General.Usuarios, pásalo real
            Username = req.Username,
            Password = hash,
            UltimoCambio = DateTime.UtcNow,
            Locked = false,
            NuevoUsuario = false,
            LoginPerpetuo = true,
            LoginClave = true,
            EstadoRegistro = 1
        };

        // 4) Transacción
        await _uow.BeginTransactionAsync();
        try
        {
            // Inserta usuario
            var res = await _uow.UsuarioSistemaRepository.InsertAsync(user, "CREATE USER");
            // Si tu InsertAsync no rellena el Id, refresca:
            if (user.Id <= 0)
            {
                // Opcional: recarga por username si hace falta
                var again = await _uow.UsuarioSistemaRepository
                    .FirstOrDefaultAsync(u => u.Username == req.Username, ct);
                user.Id = again!.Id;
            }

            // 5) Asignar roles (opcional)
            var roleNames = new List<string>();
            if (req.RolesIds is not null && req.RolesIds.Any())
            {
                // Valida que existan
                var validRoles = await _uow.RolRepository.Query()
                    .Where(r => req.RolesIds.Contains(r.Id) && r.EstadoRegistro == 1)
                    .Select(r => new { r.Id, r.Nombre })
                    .ToListAsync(ct);

                foreach (var r in validRoles)
                {
                    await _uow.RolUsuarioRepository.InsertAsync(new RolUsuario
                    {
                        IdRol = r.Id,
                        IdUsuarioSistema = user.Id,
                        EstadoRegistro = 1
                    }, "ASSIGN ROLE");

                    roleNames.Add(r.Nombre);
                }
            }

            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();

            // Auditoría mínima
            await RegistrarIntento(user.Username, user.Id, true, "Usuario registrado", ct);

            return new RegisterResponse(user.Id, user.Username, roleNames);
        }
        catch
        {
            await _uow.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        // Si prefieres tomar el userId del token:
        // var userId = int.Parse(_http.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _uow.UsuarioSistemaRepository
            .FirstOrDefaultAsync(u => u.Id == req.UserId && u.EstadoRegistro == 1, ct);

        if (user is null) throw new UnauthorizedAccessException("Usuario no encontrado");

        if (!_hasher.Verify(user.Password, req.CurrentPassword))
            throw new UnauthorizedAccessException("Password actual incorrecta");

        // (opcional) valida robustez del new password aquí

        user.Password = _hasher.Hash(req.NewPassword);
        user.UltimoCambio = DateTime.UtcNow;

        _uow.UsuarioSistemaRepository.Update(user); // si tienes UpdateAsync, úsalo
        await _uow.SaveChangesAsync();
    }



}
