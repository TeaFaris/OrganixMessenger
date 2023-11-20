namespace OrganixMessenger.ServerServices.UserAuthenticationManagerService
{
    public interface IUserAuthenticationManager
    {
        Task<ChangePasswordResult> ChangePassword(string code, string newPassword);
        Task<ForgotPasswordResult> ForgotPassword(string email);
        Task<RegisterUserResult> Register(string username, string email, string password, Role role);
        Task<ApplicationUser?> ValidateCredentials(string username, string password);
        Task<VerifyEmailResult> ConfirmEmail(string code);
    }

    public sealed class RegisterUserResult
    {
        public bool Successful { get; init; }
        public IEnumerable<string> Errors { get; init; }
        public ApplicationUser? User { get; init; }
    }

    public sealed class VerifyEmailResult
    {
        public bool Successful { get; init; }
    }

    public sealed class ChangePasswordResult
    {
        public bool Successful { get; init; }
    }

    public sealed class ForgotPasswordResult
    {
        public bool Successful { get; init; }
        public string? ErrorMessage { get; init; }
    }
}