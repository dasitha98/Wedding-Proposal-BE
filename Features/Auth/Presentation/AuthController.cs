using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Wedding_Proposal_BE.Features.Auth.Application.DTOs;
using Wedding_Proposal_BE.Features.Auth.Application.Interfaces;
using Wedding_Proposal_BE.Shared.Common;

namespace Wedding_Proposal_BE.Features.Auth.Presentation;

/// Thin controller: only orchestrates HTTP <-> the Auth application services, no business logic.
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly ISessionService _sessionService;
    private readonly IPasswordService _passwordService;
    private readonly IAccountService _accountService;
    private readonly IGoogleSignInService _googleSignInService;

    public AuthController(
        IRegistrationService registrationService,
        ISessionService sessionService,
        IPasswordService passwordService,
        IAccountService accountService,
        IGoogleSignInService googleSignInService)
    {
        _registrationService = registrationService;
        _sessionService = sessionService;
        _passwordService = passwordService;
        _accountService = accountService;
        _googleSignInService = googleSignInService;
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _registrationService.RegisterAsync(request);
        return result.IsSuccess
            ? Ok(ApiResponse<RegisterResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<RegisterResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("register/verify")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> VerifyRegistrationOtp([FromBody] VerifyRegistrationOtpRequest request)
    {
        var result = await _registrationService.VerifyRegistrationOtpAsync(request.Email, request.Code);
        return result.IsSuccess
            ? Ok(ApiResponse<AuthResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<AuthResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("register/resend")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResendRegistrationOtp([FromBody] ResendRegistrationOtpRequest request)
    {
        var result = await _registrationService.ResendRegistrationOtpAsync(request.Email);
        return result.IsSuccess
            ? Ok(ApiResponse<RegisterResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<RegisterResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _sessionService.LoginAsync(request);
        return result.IsSuccess
            ? Ok(ApiResponse<AuthResponse>.Ok(result.Value))
            : Unauthorized(ApiResponse<AuthResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _sessionService.RefreshAsync(request.RefreshToken);
        return result.IsSuccess
            ? Ok(ApiResponse<AuthResponse>.Ok(result.Value))
            : Unauthorized(ApiResponse<AuthResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request)
    {
        var result = await _sessionService.RevokeAsync(request.RefreshToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok())
            : BadRequest(ApiResponse.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("password-reset/request")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> RequestPasswordResetOtp([FromBody] RequestPasswordResetOtpRequest request)
    {
        var result = await _passwordService.RequestPasswordResetOtpAsync(request.Email);
        return result.IsSuccess
            ? Ok(ApiResponse<RequestPasswordResetOtpResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<RequestPasswordResetOtpResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("password-reset/verify")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> VerifyPasswordResetOtp([FromBody] VerifyPasswordResetOtpRequest request)
    {
        var result = await _passwordService.VerifyPasswordResetOtpAsync(request.Email, request.Code);
        return result.IsSuccess
            ? Ok(ApiResponse<VerifyPasswordResetOtpResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<VerifyPasswordResetOtpResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("password-reset/confirm")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _passwordService.ResetPasswordAsync(request.ResetToken, request.NewPassword);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok())
            : BadRequest(ApiResponse.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _passwordService.ChangePasswordAsync(CurrentUserId, request.CurrentPassword, request.NewPassword);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok())
            : BadRequest(ApiResponse.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPut("account")]
    [Authorize]
    public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountRequest request)
    {
        var result = await _accountService.UpdateAccountAsync(CurrentUserId, request);
        return result.IsSuccess
            ? Ok(ApiResponse<AuthUserResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<AuthUserResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("google/start")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> GoogleStart([FromBody] GoogleAuthStartRequest request)
    {
        var result = await _googleSignInService.StartGoogleSignInAsync(request.RedirectUri);
        return result.IsSuccess
            ? Ok(ApiResponse<GoogleAuthStartResponse>.Ok(result.Value))
            : BadRequest(ApiResponse<GoogleAuthStartResponse>.Fail(result.Error!, result.ErrorCode));
    }

    [HttpPost("google/callback")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> GoogleCallback([FromBody] GoogleAuthCallbackRequest request)
    {
        var result = await _googleSignInService.CompleteGoogleSignInAsync(request.Code, request.State);
        return result.IsSuccess
            ? Ok(ApiResponse<AuthResponse>.Ok(result.Value))
            : Unauthorized(ApiResponse<AuthResponse>.Fail(result.Error!, result.ErrorCode));
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
