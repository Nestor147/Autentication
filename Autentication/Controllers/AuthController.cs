using Autentication.Application.DTOs;
using Autentication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Autentication.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _svc;
    public AuthController(IAuthService svc) => _svc = svc;

    [HttpPost("sessions")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        try { var pair = await _svc.LoginAsync(req, ct); return Ok(pair); }
        catch (UnauthorizedAccessException) { return Unauthorized(new { message = "Credenciales inválidas" }); }
        catch (Exception ex) { return StatusCode(500, new { message = "Auth error", detail = ex.Message }); }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        try { var pair = await _svc.RefreshAsync(req, ct); return Ok(pair); }
        catch (UnauthorizedAccessException) { return Unauthorized(new { message = "Refresh inválido" }); }
        catch (Exception ex) { return StatusCode(500, new { message = "Refresh error", detail = ex.Message }); }
    }

    [HttpPost("logout")]
    [AllowAnonymous] // o [Authorize] si quieres exigir access token
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req, CancellationToken ct)
    {
        try { await _svc.LogoutAsync(req, ct); return Ok(); }
        catch (Exception ex) { return StatusCode(500, new { message = "Logout error", detail = ex.Message }); }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        try
        {
            var result = await _svc.RegisterAsync(req, ct);
            return Created($"/api/auth/users/{result.IdUsuarioSistema}", result);
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { message = "Register error", detail = ex.Message }); }
    }

    [HttpPost("password")]
    [Authorize] // requiere access token
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req, CancellationToken ct)
    {
        try { await _svc.ChangePasswordAsync(req, ct); return Ok(); }
        catch (UnauthorizedAccessException) { return Unauthorized(new { message = "Password actual incorrecta" }); }
        catch (Exception ex) { return StatusCode(500, new { message = "Password error", detail = ex.Message }); }
    }

    // JWKS en raíz (lo dejaste así, perfecto para descubrimiento OpenID)
    [HttpGet("/.well-known/jwks.json")]
    [AllowAnonymous]
    public IActionResult JwksRoot() => Content(_svc.GetJwks(), "application/json");
}
