using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Services
{
    public sealed class AccountVerificationService : IAccountVerificationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AccountVerificationService> _logger;

        public AccountVerificationService(
            IUserRepository userRepository,
            ILogger<AccountVerificationService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────

        public async Task<AccountVerificationResult> VerifyAccountAsync(
            string email,
            int tokenLoginTerm,
            CancellationToken ct = default)
        {
            // ── STEP 4 + 5: Fetch user, then validate login_term ──────────────
            // We fetch before the login_term check so we can distinguish
            // "user not found" (404) from "stale token" (401) precisely.
            var normalisedEmail = email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalisedEmail, ct);

            // ── STEP 5: User existence check → 404 ───────────────────────────
            if (user is null)
            {
                _logger.LogWarning(
                    "Account verification failed — user not found: {Email}", normalisedEmail);

                return AccountVerificationResult.Failure(
                    statusCode: 404,
                    message: "account not found",
                    code: "USER_NOT_FOUND",
                    description: "No account exists for the authenticated email.");
            }

            // ── STEP 4: login_term check → 401 ────────────────────────────────
            // The spec states: token.login_term >= user.login_term.
            // A DB-side increment (e.g. password change, forced logout) raises
            // login_term, making all tokens with a lower value invalid.
            if (tokenLoginTerm < user.LoginTerm)
            {
                _logger.LogWarning(
                    "Account verification failed — stale login_term for {Email}. " +
                    "Token: {TokenTerm}, DB: {DbTerm}.",
                    normalisedEmail, tokenLoginTerm, user.LoginTerm);

                return AccountVerificationResult.Failure(
                    statusCode: 401,
                    message: "failed verification",
                    code: "INVALID_LOGIN_TERM",
                    description: "Access token login_term is outdated. Please log in again.");
            }

            // ── STEP 6: Return user profile ───────────────────────────────────
            _logger.LogInformation(
                "Account verification succeeded for {Email}.", normalisedEmail);

            return AccountVerificationResult.Ok(new AccountVerificationResponseDto
            {
                Email = user.Email,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Address = user.Address
            });
        }
    }
    }
