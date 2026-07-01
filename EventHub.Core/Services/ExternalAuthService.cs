using System.Security.Claims;
using EventHub.Core.Contracts;
using EventHub.Core.Models.User;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Core.Services
{
    public class ExternalAuthService : IExternalAuthService
    {
        private const string DefaultRole = "Client";

        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public ExternalAuthService(
            UserManager<User> _userManager,
            RoleManager<IdentityRole> _roleManager)
        {
            userManager = _userManager;
            roleManager = _roleManager;
        }

        public async Task<ExternalLoginProcessResult> HandleExternalLoginCallbackAsync(
            ExternalLoginInfo loginInfo,
            string? clientIp,
            string loginDevice)
        {
            var linkedUser = await userManager.FindByLoginAsync(
                loginInfo.LoginProvider,
                loginInfo.ProviderKey);

            if (linkedUser != null)
            {
                if (!CanSignIn(linkedUser))
                {
                    return ExternalLoginProcessResult.Failure(
                        ExternalLoginProcessStatus.AccountInactive);
                }

                await UpdateLoginMetadataAsync(linkedUser, clientIp, loginDevice);
                return ExternalLoginProcessResult.Success(linkedUser);
            }

            var email = GetEmail(loginInfo);
            if (string.IsNullOrWhiteSpace(email))
            {
                return ExternalLoginProcessResult.Failure(
                    ExternalLoginProcessStatus.EmailUnavailable);
            }

            if (!IsEmailVerified(loginInfo))
            {
                return ExternalLoginProcessResult.Failure(
                    ExternalLoginProcessStatus.EmailNotVerified);
            }

            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return await LinkExistingUserAsync(existingUser, loginInfo, clientIp, loginDevice);
            }

            return ExternalLoginProcessResult.RequiresConfirmation(
                email,
                loginInfo.ProviderDisplayName ?? loginInfo.LoginProvider);
        }

        public async Task<ExternalLoginProcessResult> ConfirmExternalLoginAsync(
            ExternalLoginInfo loginInfo,
            ExternalLoginConfirmationViewModel model,
            string? clientIp,
            string loginDevice)
        {
            var email = GetEmail(loginInfo);
            if (string.IsNullOrWhiteSpace(email))
            {
                return ExternalLoginProcessResult.Failure(
                    ExternalLoginProcessStatus.EmailUnavailable);
            }

            if (!IsEmailVerified(loginInfo))
            {
                return ExternalLoginProcessResult.Failure(
                    ExternalLoginProcessStatus.EmailNotVerified);
            }

            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return await LinkExistingUserAsync(existingUser, loginInfo, clientIp, loginDevice);
            }

            if (await userManager.FindByNameAsync(model.UserName) != null)
            {
                return ExternalLoginProcessResult.Failure(
                    ExternalLoginProcessStatus.DuplicateUserName);
            }

            var now = DateTime.UtcNow;
            var user = new User
            {
                UserName = model.UserName,
                Email = email,
                EmailConfirmed = true,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return ExternalLoginProcessResult.Failure(
                    ExternalLoginProcessStatus.Failed,
                    JoinErrors(createResult));
            }

            var addLoginResult = await userManager.AddLoginAsync(user, loginInfo);
            if (!addLoginResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return ExternalLoginProcessResult.Failure(
                    ExternalLoginProcessStatus.Failed,
                    JoinErrors(addLoginResult));
            }

            if (!await roleManager.RoleExistsAsync(DefaultRole))
            {
                await userManager.DeleteAsync(user);
                return ExternalLoginProcessResult.Failure(
                    ExternalLoginProcessStatus.Failed,
                    $"Role '{DefaultRole}' does not exist.");
            }

            var roleResult = await userManager.AddToRoleAsync(user, DefaultRole);
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return ExternalLoginProcessResult.Failure(
                    ExternalLoginProcessStatus.Failed,
                    JoinErrors(roleResult));
            }

            await UpdateLoginMetadataAsync(user, clientIp, loginDevice);
            return ExternalLoginProcessResult.Success(user);
        }

        private async Task<ExternalLoginProcessResult> LinkExistingUserAsync(
            User user,
            ExternalLoginInfo loginInfo,
            string? clientIp,
            string loginDevice)
        {
            if (!CanSignIn(user))
            {
                return ExternalLoginProcessResult.Failure(
                    ExternalLoginProcessStatus.AccountInactive);
            }

            var addLoginResult = await userManager.AddLoginAsync(user, loginInfo);
            if (!addLoginResult.Succeeded)
            {
                return ExternalLoginProcessResult.Failure(
                    ExternalLoginProcessStatus.Failed,
                    JoinErrors(addLoginResult));
            }

            await UpdateLoginMetadataAsync(user, clientIp, loginDevice);
            return ExternalLoginProcessResult.Success(user);
        }

        private async Task UpdateLoginMetadataAsync(
            User user,
            string? clientIp,
            string loginDevice)
        {
            user.LastLoginIP = clientIp;
            user.LastLoginDevice = loginDevice;
            user.LastOnline = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await userManager.UpdateAsync(user);
        }

        private static bool CanSignIn(User user)
            => user.IsActive && !user.IsDeleted;

        private static string? GetEmail(ExternalLoginInfo loginInfo)
            => loginInfo.Principal.FindFirstValue(ClaimTypes.Email)
               ?? loginInfo.Principal.FindFirstValue("email");

        private static bool IsEmailVerified(ExternalLoginInfo loginInfo)
        {
            var value = loginInfo.Principal.FindFirstValue("email_verified");

            return string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(value, "1", StringComparison.Ordinal);
        }

        private static string JoinErrors(IdentityResult result)
            => string.Join("; ", result.Errors.Select(e => e.Description));
    }
}
