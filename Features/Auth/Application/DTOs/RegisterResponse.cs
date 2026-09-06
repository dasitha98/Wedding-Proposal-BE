namespace Wedding_Proposal_BE.Features.Auth.Application.DTOs;

/// Returned after Register/ResendRegistrationOtp: registration created the (unverified) account
/// and an OTP was emailed, but no auth tokens are issued until VerifyRegistrationOtp succeeds.
public record RegisterResponse(string Email, int OtpExpiresInSeconds, int ResendAvailableInSeconds);
