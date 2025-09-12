using Autentication.Application.DTOs;
using Autentication.Application.DTOs.Atacado;
using Autentication.Application.Interfaces;
using Autentication.Core.Entities.Core;
using Autentication.Web.Security;
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

    [HttpPost("register-buyer")]
    public async Task<ActionResult<TokenPair>> RegisterBuyer([FromBody] RegisterBuyerRequest request, CancellationToken ct)
    {
        try
        {
            var tokens = await _svc.RegisterBuyerAtacadoAsync(request, ct);
            return Ok(tokens);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno al registrar comprador.", detail = ex.Message });
        }
    }

    [HttpPost("sellers")]
    [InternalOnly]
    public async Task<ActionResult<ApiOk<CreateSellerResponse>>> CreateSeller([FromBody] CreateSellerRequest req, CancellationToken ct)
    {
        var result = await _svc.CreateSellerAsync(req, ct); // si hay duplicado → UserDuplicateException
        return Created($"/api/users/{result.UserId}", new ApiOk<CreateSellerResponse> { Data = result });
    }


    [HttpPost("password")]
    [Authorize] // requiere access token
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req, CancellationToken ct)
    {
        try { await _svc.ChangePasswordAsync(req, ct); return Ok(); }
        catch (UnauthorizedAccessException) { return Unauthorized(new { message = "Password actual incorrecta" }); }
        catch (Exception ex) { return StatusCode(500, new { message = "Password error", detail = ex.Message }); }
    }


    [HttpPost("buyers")]
    [InternalOnly]
    public async Task<ActionResult<CreateBuyerResponse>> CreateBuyer([FromBody] CreateBuyerRequest req, CancellationToken ct)
    {
        try
        {
            var result = await _svc.CreateBuyerAsync(req, ct);
            return Created($"/api/users/{result.UserId}", result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { message = "Unexpected server error." });
        }
    }

    [HttpPost("admins")]
    [InternalOnly]
    public async Task<ActionResult<CreateAdminResponse>> CreateAdmin([FromBody] CreateAdminRequest req, CancellationToken ct)
    {
        try
        {
            var result = await _svc.CreateAdminAsync(req, ct);
            return Created($"/api/users/{result.UserId}", result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { message = "Unexpected server error." });
        }
    }

    // JWKS en raíz (lo dejaste así, perfecto para descubrimiento OpenID)
    [HttpGet("/.well-known/jwks.json")]
    [AllowAnonymous]
    public IActionResult JwksRoot() => Content(_svc.GetJwks(), "application/json");
}
