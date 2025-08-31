using Autentication.Application.DTOs;
using Autentication.Application.DTOs.Atacado;
using Autentication.Application.Interfaces;
using Autentication.Application.Interfaces.Jwt;
using Autentication.Application.Jwt;
using Autentication.Application.Password;
using Autentication.Core.Entities.Autorizacion;
using Autentication.Core.Interfaces.Core;
using Autentication.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

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

        if (user is null)
        {
            await RegistrarIntento(req.Username, null, false, "Usuario no existe", ct);
            throw new UnauthorizedAccessException();
        }

        if (user.Locked)
        {
            await RegistrarIntento(req.Username, user.Id, false, "Usuario bloqueado", ct);
            throw new UnauthorizedAccessException();
        }

        // 2) Password
        if (!_hasher.Verify(user.Password, req.Password))
        {
            await RegistrarIntento(req.Username, user.Id, false, "Password inválido", ct);
            throw new UnauthorizedAccessException();
        }

        // 3) Roles  (JOIN: RolesUsuarios -> Rol)

        // 3) Roles  (JOIN: RolesUsuarios -> Rol)
        var appsWithRoles = await GetUserAppRolesAsync(user.Id, ct);
        var roleNames = appsWithRoles.SelectMany(a => a.Roles).Distinct().ToList();

        // 4) Access JWT con claims adicionales
        var jti = Guid.NewGuid().ToString("N");

        // 🔁 Construye claims correctos (incluye app_roles/apps como JSON)
        // 🔁 Construye claims correctos (incluye app_roles/apps como JSON)
        var claims = await BuildAccessClaimsAsync(user, ct);

        // agrega roles planos (ClaimTypes.Role) y también el claim "roles" (JSON) para el front
        foreach (var r in roleNames)
            claims.Add(new Claim(ClaimTypes.Role, r));

        claims.Add(new Claim("roles",
            JsonSerializer.Serialize(roleNames),
            Microsoft.IdentityModel.JsonWebTokens.JsonClaimValueTypes.Json));


        var access = _jwt.CreateAccessToken(new TokenDescriptor(
            Subject: user.Id.ToString(),
            Roles: null, // <- setea null si vas a pasar Claims manuales
            Issuer: _cfg["Auth:Issuer"]!,
            Audience: _cfg["Auth:Audience"]!,
            Jti: jti,
            Lifetime: TimeSpan.FromMinutes(10),
            Claims: null,             // <- no uses el diccionario para objetos
            ExtraClaims: claims       // <- pasa la List<Claim>
        ));

        // 5) Refresh opaco (guardar HASH en BD)
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
        // Roles planos
        var roleNames = await _uow.RolUsuarioRepository.Query()
            .Where(ru => ru.IdUsuarioSistema == user.Id && ru.EstadoRegistro == 1)
            .Join(_uow.RolRepository.Query().Where(r => r.EstadoRegistro == 1),
                  ru => ru.IdRol, r => r.Id, (ru, r) => r.Nombre)
            .Distinct()
            .ToListAsync(ct);

        // 🔁 Recalcula claims completos (incluye app_roles/apps como JSON)
        var claims = await BuildAccessClaimsAsync(user, ct);
        foreach (var r in roleNames)
            claims.Add(new Claim(ClaimTypes.Role, r));

        claims.Add(new Claim("roles",
            JsonSerializer.Serialize(roleNames),
            Microsoft.IdentityModel.JsonWebTokens.JsonClaimValueTypes.Json));


        token.Usado = true;
        token.Revocado = true;

        var access = _jwt.CreateAccessToken(new TokenDescriptor(
            Subject: user.Id.ToString(),
            Roles: null,
            Issuer: _cfg["Auth:Issuer"]!,
            Audience: _cfg["Auth:Audience"]!,
            Jti: Guid.NewGuid().ToString("N"),
            Lifetime: TimeSpan.FromDays(15),
            Claims: null,
            ExtraClaims: claims
        ));


        // Rotar el refresh actual


        // Nuevo access + refresh


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

    public async Task<TokenPair> RegisterBuyerAtacadoAsync(RegisterBuyerRequest req, CancellationToken ct)
    {
        // 0) Validaciones básicas
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            throw new InvalidOperationException("Username y Password son requeridos.");

        // 1) Resolver aplicación ATACADO (lecturas FUERA de transacción y sin tracking)
        var aplicacion = await _uow.AplicacionRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Sigla == "ATACADO" && a.EstadoRegistro == 1, ct)
            ?? throw new InvalidOperationException("La aplicación ATACADO no existe o está inactiva.");

        // 2) Resolver rol COMPRADOR en esa app (lectura FUERA de transacción y sin tracking)
        var rol = await _uow.RolRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdAplicacion == aplicacion.Id
                                   && r.EstadoRegistro == 1
                                   && r.Nombre.ToUpper() == "COMPRADOR", ct)
            ?? throw new InvalidOperationException("No existe el rol COMPRADOR para ATACADO.");

        // 3) Validar duplicados (igual que RegisterAsync)
        var exists = await _uow.UsuarioSistemaRepository
            .FirstOrDefaultAsync(u => u.Username == req.Username && u.EstadoRegistro == 1, ct);
        if (exists is not null)
            throw new InvalidOperationException("El username ya está registrado.");

        // 4) Hash de password (igual que RegisterAsync)
        var hash = _hasher.Hash(req.Password);

        // 5) Construir entidad (igual que RegisterAsync)
        var user = new UsuarioSistema
        {
            IdUsuarioGeneral = 0,
            Username = req.Username.Trim(),
            Password = hash,
            UltimoCambio = DateTime.UtcNow,
            Locked = false,
            NuevoUsuario = false,
            LoginPerpetuo = true,
            LoginClave = true,
            EstadoRegistro = 1
        };

        // 6) Transacción SOLO para escrituras (igual que RegisterAsync)
        await _uow.BeginTransactionAsync();
        try
        {
            // Inserta usuario
            await _uow.UsuarioSistemaRepository.InsertAsync(user, "CREATE USER");

            // Si el Insert no setea Id, refrescar por username
            if (user.Id <= 0)
            {
                var again = await _uow.UsuarioSistemaRepository
                    .FirstOrDefaultAsync(u => u.Username == user.Username, ct);
                user.Id = again!.Id;
            }

            // Asignar rol COMPRADOR
            await _uow.RolUsuarioRepository.InsertAsync(new RolUsuario
            {
                IdRol = rol.Id,
                IdUsuarioSistema = user.Id,
                EstadoRegistro = 1
            }, "ASSIGN ROLE");

            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();
        }
        catch
        {
            await _uow.RollbackTransactionAsync();
            throw;
        }

        // 7) Emitir tokens (access + refresh) como en Login/Refresh
        // 7) Emitir tokens (access + refresh)
        var jti = Guid.NewGuid().ToString("N");
        var rolesDelUsuario = new List<string> { rol.Nombre };

        // Reutiliza el mismo helper para armar claims completos
        var claims = await BuildAccessClaimsAsync(user, ct);

        // ClaimTypes.Role + claim "roles" (JSON) para el front
        claims.Add(new Claim(ClaimTypes.Role, rol.Nombre));
        claims.Add(new Claim("roles",
            JsonSerializer.Serialize(rolesDelUsuario),
            Microsoft.IdentityModel.JsonWebTokens.JsonClaimValueTypes.Json));

        var access = _jwt.CreateAccessToken(new TokenDescriptor(
            Subject: user.Id.ToString(),
            Roles: null,
            Issuer: _cfg["Auth:Issuer"]!,
            Audience: _cfg["Auth:Audience"]!,
            Jti: jti,
            Lifetime: TimeSpan.FromMinutes(10),
            Claims: null,
            ExtraClaims: claims
        ));


        var opaque = RefreshTokenHasher.GenerateOpaque();
        var rHash = RefreshTokenHasher.Hash(opaque);

        await _uow.RefreshTokenRepository.InsertAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            IdUsuarioSistema = user.Id,
            TokenHash = rHash,
            FechaExpiracion = DateTime.UtcNow.AddDays(15),
            IP = GetClientIp(),
            UserAgent = GetUserAgent()
        });
        await _uow.SaveChangesAsync();

        // Auditoría mínima
        await RegistrarIntento(user.Username, user.Id, true, "Usuario comprador ATACADO registrado", ct);

        return new TokenPair(access, opaque);
    }

    private async Task<List<AppRolesDto>> GetUserAppRolesAsync(int userId, CancellationToken ct)
    {
        // 1) Consulta PLANA traducible 100% a SQL
        var rows = await (
            from ru in _uow.RolUsuarioRepository.Query().AsNoTracking()
            where ru.IdUsuarioSistema == userId && ru.EstadoRegistro == 1
            join r in _uow.RolRepository.Query().AsNoTracking().Where(x => x.EstadoRegistro == 1)
                on ru.IdRol equals r.Id
            join a in _uow.AplicacionRepository.Query().AsNoTracking().Where(x => x.EstadoRegistro == 1)
                on r.IdAplicacion equals a.Id
            select new
            {
                a.Id,
                a.Sigla,
                a.Descripcion,
                Rol = r.Nombre
            }
        ).ToListAsync(ct); // ⬅️ materializamos aquí

        // 2) Agrupamos en memoria sin problemas de traducción
        var result = rows
            .GroupBy(x => new { x.Id, x.Sigla, x.Descripcion })
            .Select(g => new AppRolesDto(
                g.Key.Id,
                g.Key.Sigla,
                g.Key.Descripcion,
                g.Select(x => x.Rol).Distinct().OrderBy(n => n).ToList()
            ))
            .OrderBy(x => x.Sigla)
            .ToList();

        return result;
    }

    private async Task<List<Claim>> BuildAccessClaimsAsync(UsuarioSistema user, CancellationToken ct)
    {
        // Obtén apps + roles por app (ya tienes este método)
        var appsWithRoles = await GetUserAppRolesAsync(user.Id, ct);

        var appRoles = appsWithRoles.Select(a => new {
            id = a.IdAplicacion,
            sigla = a.Sigla,
            roles = a.Roles
        }).ToList();

        var appsMini = appsWithRoles.Select(a => new { id = a.IdAplicacion, sigla = a.Sigla }).ToList();

        var claims = new List<Claim>
    {
        new Claim("name", user.Username),
        new Claim("preferred_username", user.Username),
        new Claim("idUsuarioGeneral", user.IdUsuarioGeneral.ToString()),
        new Claim("mfa", "false"),
        new Claim("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),

        // 👇 serializa como JSON y marca el tipo del claim
        new Claim("app_roles", JsonSerializer.Serialize(appRoles), JsonClaimValueTypes.Json),
        new Claim("apps", JsonSerializer.Serialize(appsMini), JsonClaimValueTypes.Json)
    };

        return claims;
    }

    public async Task<CreateSellerResponse> CreateSellerAsync(CreateSellerRequest req, CancellationToken ct)
    {
        // Basic input validation
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            throw new InvalidOperationException("Username and Password are required.");

        try
        {
            // Resolve ATACADO application (read, no tracking)
            var app = await _uow.AplicacionRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Sigla == "ATACADO" && a.EstadoRegistro == 1, ct)
                ?? throw new InvalidOperationException("Application ATACADO does not exist or is inactive.");

            // Resolve VENDEDOR role for ATACADO (read, no tracking)
            var role = await _uow.RolRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdAplicacion == app.Id
                                       && r.EstadoRegistro == 1
                                       && r.Nombre.ToUpper() == "VENDEDOR", ct)
                ?? throw new InvalidOperationException("Role VENDEDOR does not exist for ATACADO.");

            // Check duplicates
            var exists = await _uow.UsuarioSistemaRepository
                .FirstOrDefaultAsync(u => u.Username == req.Username && u.EstadoRegistro == 1, ct);
            if (exists is not null)
                throw new InvalidOperationException("The username is already registered.");

            // Hash password
            var hash = _hasher.Hash(req.Password);

            // Build entity
            var user = new UsuarioSistema
            {
                IdUsuarioGeneral = req.IdUsuarioGeneral ?? 0,
                Username = req.Username.Trim(),
                Password = hash,
                UltimoCambio = DateTime.UtcNow,
                Locked = false,
                NuevoUsuario = false,
                LoginPerpetuo = true,
                LoginClave = true,
                EstadoRegistro = 1
            };

            await _uow.BeginTransactionAsync();

            try
            {
                // Insert user
                await _uow.UsuarioSistemaRepository.InsertAsync(user, "CREATE SELLER USER");

                // Ensure Id (if not set by repo)
                if (user.Id <= 0)
                {
                    var again = await _uow.UsuarioSistemaRepository
                        .FirstOrDefaultAsync(u => u.Username == user.Username, ct);
                    user.Id = again!.Id;
                }

                // Assign VENDEDOR role
                await _uow.RolUsuarioRepository.InsertAsync(new RolUsuario
                {
                    IdRol = role.Id,
                    IdUsuarioSistema = user.Id,
                    EstadoRegistro = 1
                }, "ASSIGN SELLER ROLE");

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }

            // Minimal audit
            await RegistrarIntento(user.Username, user.Id, true, "Seller created (no tokens issued)", ct);

            return new CreateSellerResponse
            {
                UserId = user.Id,
                Username = user.Username,
                RoleAssigned = "VENDEDOR"
            };
        }
        catch
        {
            // Opcional: aquí podrías mapear a una excepción de dominio propia
            throw;
        }
    }

    public async Task<CreateBuyerResponse> CreateBuyerAsync(CreateBuyerRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            throw new InvalidOperationException("Username and Password are required.");

        try
        {
            // 1) App ATACADO
            var app = await _uow.AplicacionRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Sigla == "ATACADO" && a.EstadoRegistro == 1, ct)
                ?? throw new InvalidOperationException("Application ATACADO does not exist or is inactive.");

            // 2) Rol COMPRADOR
            var role = await _uow.RolRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdAplicacion == app.Id
                                       && r.EstadoRegistro == 1
                                       && r.Nombre.ToUpper() == "COMPRADOR", ct)
                ?? throw new InvalidOperationException("Role COMPRADOR does not exist for ATACADO.");

            // 3) Duplicados (solo activos)
            var exists = await _uow.UsuarioSistemaRepository
                .FirstOrDefaultAsync(u => u.Username == req.Username && u.EstadoRegistro == 1, ct);
            if (exists is not null)
                throw new InvalidOperationException("The username is already registered.");

            // 4) Hash password
            var hash = _hasher.Hash(req.Password);

            // 5) Entidad
            var user = new UsuarioSistema
            {
                IdUsuarioGeneral = req.IdUsuarioGeneral ?? 0,
                Username = req.Username.Trim(),
                Password = hash,
                UltimoCambio = DateTime.UtcNow,
                Locked = false,
                NuevoUsuario = false,
                LoginPerpetuo = true,
                LoginClave = true,
                EstadoRegistro = 1
            };

            // 6) Escrituras en transacción
            await _uow.BeginTransactionAsync();
            try
            {
                await _uow.UsuarioSistemaRepository.InsertAsync(user, "CREATE BUYER USER");

                if (user.Id <= 0)
                {
                    var again = await _uow.UsuarioSistemaRepository
                        .FirstOrDefaultAsync(u => u.Username == user.Username, ct);
                    user.Id = again!.Id;
                }

                await _uow.RolUsuarioRepository.InsertAsync(new RolUsuario
                {
                    IdRol = role.Id,
                    IdUsuarioSistema = user.Id,
                    EstadoRegistro = 1
                }, "ASSIGN BUYER ROLE");

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }

            // Auditoría mínima
            await RegistrarIntento(user.Username, user.Id, true, "Buyer created (no tokens issued)", ct);

            return new CreateBuyerResponse
            {
                UserId = user.Id,
                Username = user.Username,
                RoleAssigned = "COMPRADOR"
            };
        }
        catch
        {
            throw; // deja que el controller mapee a HTTP
        }
    }


    public async Task<CreateAdminResponse> CreateAdminAsync(CreateAdminRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            throw new InvalidOperationException("Username and Password are required.");

        try
        {
            // 1) App ATACADO
            var app = await _uow.AplicacionRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Sigla == "ATACADO" && a.EstadoRegistro == 1, ct)
                ?? throw new InvalidOperationException("Application ATACADO does not exist or is inactive.");

            // 2) Rol ADMINISTRADOR (soporta también "ADMIN")
            var role = await _uow.RolRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdAplicacion == app.Id
                                       && r.EstadoRegistro == 1
                                       && (r.Nombre.ToUpper() == "ADMINISTRADOR" || r.Nombre.ToUpper() == "ADMIN"), ct)
                ?? throw new InvalidOperationException("Role ADMINISTRADOR/ADMIN does not exist for ATACADO.");

            // 3) Duplicados (solo activos; ajusta si quieres bloquear también inactivos)
            var exists = await _uow.UsuarioSistemaRepository
                .FirstOrDefaultAsync(u => u.Username == req.Username && u.EstadoRegistro == 1, ct);
            if (exists is not null)
                throw new InvalidOperationException("The username is already registered.");

            // 4) Hash password
            var hash = _hasher.Hash(req.Password);

            // 5) Entidad
            var user = new UsuarioSistema
            {
                IdUsuarioGeneral = req.IdUsuarioGeneral ?? 0,
                Username = req.Username.Trim(),
                Password = hash,
                UltimoCambio = DateTime.UtcNow,
                Locked = false,
                NuevoUsuario = false,
                LoginPerpetuo = true,
                LoginClave = true,
                EstadoRegistro = 1
            };

            // 6) Escrituras en transacción
            await _uow.BeginTransactionAsync();
            try
            {
                await _uow.UsuarioSistemaRepository.InsertAsync(user, "CREATE ADMIN USER");

                if (user.Id <= 0)
                {
                    var again = await _uow.UsuarioSistemaRepository
                        .FirstOrDefaultAsync(u => u.Username == user.Username, ct);
                    user.Id = again!.Id;
                }

                await _uow.RolUsuarioRepository.InsertAsync(new RolUsuario
                {
                    IdRol = role.Id,
                    IdUsuarioSistema = user.Id,
                    EstadoRegistro = 1
                }, "ASSIGN ADMIN ROLE");

                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }

            // Auditoría mínima
            await RegistrarIntento(user.Username, user.Id, true, "Admin created (no tokens issued)", ct);

            return new CreateAdminResponse
            {
                UserId = user.Id,
                Username = user.Username,
                RoleAssigned = role.Nombre.ToUpper()
            };
        }
        catch
        {
            throw; // deja que el controller mapee a HTTP
        }
    }


}
