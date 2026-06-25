using EventHubUser = EventHub.Infrastructure.Data.Models.User;

namespace EventHub.Core.Models.User
{
    public enum ExternalLoginProcessStatus
    {
        Succeeded,
        RequiresConfirmation,
        AccountInactive,
        EmailUnavailable,
        EmailNotVerified,
        DuplicateUserName,
        Failed
    }

    public class ExternalLoginProcessResult
    {
        public ExternalLoginProcessStatus Status { get; private init; }

        public EventHubUser? User { get; private init; }

        public string? Email { get; private init; }

        public string? Provider { get; private init; }

        public string? Error { get; private init; }

        public bool Succeeded => Status == ExternalLoginProcessStatus.Succeeded;

        public static ExternalLoginProcessResult Success(EventHubUser user)
            => new()
            {
                Status = ExternalLoginProcessStatus.Succeeded,
                User = user
            };

        public static ExternalLoginProcessResult RequiresConfirmation(string email, string provider)
            => new()
            {
                Status = ExternalLoginProcessStatus.RequiresConfirmation,
                Email = email,
                Provider = provider
            };

        public static ExternalLoginProcessResult Failure(
            ExternalLoginProcessStatus status,
            string? error = null)
            => new()
            {
                Status = status,
                Error = error
            };
    }
}
