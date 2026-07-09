using ChipoBackend.Application.Features.Auth.Commands.ChangePassword;
using ChipoBackend.Application.Features.Auth.Commands.Login;
using ChipoBackend.Application.Features.Auth.Commands.RefreshToken;
using ChipoBackend.Application.Features.Auth.Commands.Register;
using ChipoBackend.Application.Features.Auth.Commands.RevokeToken;
using ChipoBackend.Application.Features.Auth.Queries.GetCurrentUser;
using ChipoBackend.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/auth")]
public class AuthController(ICurrentUserService currentUserService) : BaseApiController
{
    // ── Registro ──────────────────────────────────────────────────────────────

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RegisterCommand(request.Email, request.Password, request.FirstName, request.LastName, request.PhoneNumber), ct);
        return CreatedAtAction(nameof(Register), result);
    }

    // ── Login / Logout ────────────────────────────────────────────────────────

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await Mediator.Send(new LoginCommand(request.Email, request.Password, ip), ct);
        return Ok(result);
    }

    /// <summary>
    /// Cierra la sesión revocando el refresh token dado.
    /// POST /api/auth/logout
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await Mediator.Send(new RevokeTokenCommand(request.RefreshToken), ct);
        return NoContent();
    }

    // ── Token ─────────────────────────────────────────────────────────────────

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await Mediator.Send(new RefreshTokenCommand(request.RefreshToken, ip), ct);
        return Ok(result);
    }

    /// <summary>
    /// Revoca explícitamente un refresh token (alias semántico de logout).
    /// POST /api/auth/revoke-token
    /// </summary>
    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request, CancellationToken ct)
    {
        await Mediator.Send(new RevokeTokenCommand(request.RefreshToken), ct);
        return NoContent();
    }

    // ── Perfil del usuario autenticado ────────────────────────────────────────

    /// <summary>
    /// Devuelve los datos del usuario autenticado (extraídos del JWT + DB).
    /// GET /api/auth/me
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCurrentUserQuery(), ct);
        return Ok(result);
    }

    // ── Contraseña ────────────────────────────────────────────────────────────

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = currentUserService.UserId!.Value;
        await Mediator.Send(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword), ct);
        return NoContent();
    }
}

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string? PhoneNumber);
public record LoginRequest(string Email, string Password);
public record LogoutRequest(string RefreshToken);
public record RefreshTokenRequest(string RefreshToken);
public record RevokeTokenRequest(string RefreshToken);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
