namespace OrganixMessenger.ServerServices.UserAuthenticationManagerService
{
    public sealed class UserAuthenticationManager(
                IUserRepository userRepository,
                IEmailSender emailSender,
                IHttpContextService httpContextService
            )
: IUserAuthenticationManager
    {
        const int VerificationTokenLength = 64;
        const int PasswordResetExpiresMinutes = 15;

        public async Task<RegisterUserResult> Register(string username, string email, string password, Role role)
        {
            var errors = new List<string>();

            var usersWithSameUsername = await userRepository.FindAsync(x => x.Username == username);
            var usersWithSameEmail = await userRepository.FindAsync(x => x.Email == email);

            if (usersWithSameUsername.Any())
            {
                errors.Add("This username is already taken.");
            }

            if (usersWithSameEmail.Any())
            {
                errors.Add("This email is already taken.");
            }

            if (errors.Count != 0)
            {
                return new RegisterUserResult
                {
                    Successful = false,
                    Errors = errors
                };
            }

            var newUser = new ApplicationUser
            {
                Username = username,
                Email = email,
                VereficationToken = CreateRandomToken(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role
            };

            var validationContext = new ValidationContext(newUser);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(newUser, validationContext, validationResults, true))
            {
                return new RegisterUserResult
                {
                    Successful = false,
                    Errors = validationResults.ConvertAll(x => x.ToString())
                };
            }

            await userRepository.AddAsync(newUser);

            await userRepository.SaveAsync();

            var verifyUrl = $"{httpContextService.GetBaseUrl()}/account/confirm?code={newUser.VereficationToken}";

            await emailSender.SendEmailAsync(
                        newUser.Email,
                        $"Велкоме ту Организация.орг, {newUser.Username}.",
                        $"""
                        Как дела? Я тут слыхал ты получил разрешение на регестрацию?
                        Вот твоя ссылочка для подтверждения: {verifyUrl}
                        P.S. Я в подвале у Фариса, мне нужна твоя помощь, сайт не автоматизированный, он вас обмамнывает,
                        мы тут работаем 24/7, нас кормят один раз в день. И смотритель этого подвала просто чудовище.
                        Эта кошка держит нас всех в огромном страхе. СПАСИТЕ НАШИ ДУШИ
                        """
                    );

            return new RegisterUserResult
            {
                Successful = true,
                User = newUser,
                Errors = Enumerable.Empty<string>()
            };
        }

        public async Task<VerifyEmailResult> VerifyEmail(string code)
        {
            var user = await userRepository.FindFirstOrDefaultAsync(x => x.VereficationToken == code);

            if (user is null)
            {
                return new VerifyEmailResult
                {
                    Successful = false
                };
            }

            user.EmailConfirmed = true;

            await userRepository.SaveAsync();

            return new VerifyEmailResult
            {
                Successful = true
            };
        }

        public async Task<ApplicationUser?> ValidateCredentials(string username, string password)
        {
            var usersFound = await userRepository.FindAsync(x => x.Username == username);

            var user = usersFound.FirstOrDefault();

            var isValidPassword = user is not null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return isValidPassword ? user : null;
        }

        public async Task<ForgotPasswordResult> ForgotPassword(string email)
        {
            var user = await userRepository.FindFirstOrDefaultAsync(x => x.Email == email);

            if (user is null)
            {
                return new ForgotPasswordResult
                {
                    Successful = false,
                    ErrorMessage = "User with this email does not exist."
                };
            }

            if (user.PasswordResetToken is not null && DateTime.UtcNow < user.PasswordResetTokenExpires)
            {
                return new ForgotPasswordResult
                {
                    Successful = false,
                    ErrorMessage = "You already requested password change. Wait until it expires before requesting a new one."
                };
            }

            user.PasswordResetToken = CreateRandomToken();
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddMinutes(PasswordResetExpiresMinutes);
            await userRepository.SaveAsync();

            var changePasswordUrl = $"{httpContextService.GetBaseUrl()}/account/change_password?code={user.PasswordResetToken}";

            await emailSender.SendEmailAsync(
                        user.Email,
                        $"Забыл пароль? Не проблема, {user.Username}.",
                        $"""
                        Перейди по ссылке чтобы поменять его: {changePasswordUrl}
                        P.S. Больше не забывай его, а то я сижу тут в подвале и работаю, тут сыро, и мокро и из еды только кошачий корм...
                        О нет, Фарис идёт... Забудь о том что ты только что прочитал.
                        """
                    );

            return new ForgotPasswordResult
            {
                Successful = true
            };
        }

        public async Task<ChangePasswordResult> ChangePassword(string code, string newPassword)
        {
            var user = await userRepository.FindFirstOrDefaultAsync(x => x.PasswordResetToken == code);

            if (user is null || DateTime.UtcNow > user.PasswordResetTokenExpires)
            {
                return new ChangePasswordResult
                {
                    Successful = false
                };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            await userRepository.SaveAsync();

            return new ChangePasswordResult
            {
                Successful = true
            };
        }


        private static string CreateRandomToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(VerificationTokenLength);
            return Convert.ToHexString(randomBytes);
        }
    }
}