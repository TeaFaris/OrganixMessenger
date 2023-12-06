using OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationUserModel;

namespace OrganixMessenger.ServerServices.UserAuthenticationManagerServices
{
    public interface IUserAuthenticationManager
    {
        Task<ChangePasswordResult> ChangePasswordAsync(string code, string newPassword);
        Task<ForgotPasswordResult> ForgotPasswordAsync(string email);
        Task<RegisterUserResult> RegisterAsync(string username, string email, string password, Role role);
        Task<ApplicationUser?> ValidateCredentialsAsync(string username, string password);
        Task<ConfirmEmailResult> ConfirmEmailAsync(string code);
    }

    public sealed class RegisterUserResult
    {
        public bool Successful { get; init; }
        public IEnumerable<string> Errors { get; init; }
        public ApplicationUser? User { get; init; }
    }

    public sealed class ConfirmEmailResult
    {
        public bool Successful { get; init; }
        public ApplicationUser? User { get; init; }
    }

    public sealed class ChangePasswordResult
    {
        public bool Successful { get; init; }
        public ApplicationUser? User { get; init; }
    }

    public sealed class ForgotPasswordResult
    {
        public bool Successful { get; init; }
        public string? ErrorMessage { get; init; }
    }
}