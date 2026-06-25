using EventHub.Core.Models.User;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Core.Contracts
{
    public interface IExternalAuthService
    {
        Task<ExternalLoginProcessResult> HandleExternalLoginCallbackAsync(
            ExternalLoginInfo loginInfo,
            string? clientIp,
            string loginDevice);

        Task<ExternalLoginProcessResult> ConfirmExternalLoginAsync(
            ExternalLoginInfo loginInfo,
            ExternalLoginConfirmationViewModel model,
            string? clientIp,
            string loginDevice);
    }
}
