namespace OrganixMessenger.ServerServices.UserAuthenticationManagerService
{
    public sealed class UserAuthenticationManager(
                IUserRepository userRepository,
                IEmailSender emailSender,
                IHttpContextService httpContextService
            ) : IUserAuthenticationManager
    {
        const int VerificationTokenLength = 64;
        const int PasswordResetExpiresMinutes = 15;

        public async Task<RegisterUserResult> RegisterAsync(string username, string email, string password, Role role)
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

            var verifyUrl = $"{httpContextService.GetBaseUrl()}/account/confirm?code={newUser.VereficationToken}";

            try
            {
                await emailSender.SendEmailAsync(
                    newUser.Email,
                    $"Велкоме ту Организация.орг, {newUser.Username}.",
                    $$"""
                        <html>
                        <head>
                            <style>
                                body {
                                    font - family: Arial, sans-serif;
                                    background-color: #f0f0f0;
                                }
                                h1 {
                                    color: #333333;
                                    text-align: center;
                                }
                                p {
                                    color: #555555;
                                    margin: 10px;
                                }
                                a {
                                    color: #0066cc;
                                    text-decoration: none;
                                }
                                a:hover {
                                    text - decoration: underline;
                                }
                            </style>
                        </head>
                        <body>
                            <h1>Привет, друг!</h1>
                            <p>Как дела? Я тут слыхал ты получил разрешение на регистрацию?</p>
                            <p>Вот твоя ссылочка для подтверждения: <a href="{{verifyUrl}}">Жмакни сюды</a></p>
                            <p>P.S. Я в подвале у Фариса, мне нужна твоя помощь, сайт не автоматизированный, он вас обманывает,
                            мы тут работаем 24/7, нас кормят один раз в день. И смотритель этого подвала просто чудовище.
                            Эта кошка держит нас всех в огромном страхе. СПАСИТЕ НАШИ ДУШИ</p>
                        </body>
                        </html>
                        """
                );
            }
            catch (Exception ex) when (ex is SmtpCommandException or ParseException)
            {
                return new RegisterUserResult
                {
                    Successful = false,
                    Errors = [ "Email doesn't exist." ]
                };
            }

            await userRepository.SaveAsync();

            return new RegisterUserResult
            {
                Successful = true,
                User = newUser,
                Errors = Enumerable.Empty<string>()
            };
        }

        public async Task<ConfirmEmailResult> ConfirmEmailAsync(string code)
        {
            var user = await userRepository.FindFirstOrDefaultAsync(x => x.VereficationToken == code);

            if (user is null)
            {
                return new ConfirmEmailResult
                {
                    Successful = false
                };
            }

            user.EmailConfirmed = true;

            await userRepository.SaveAsync();

            return new ConfirmEmailResult
            {
                Successful = true,
                User = user
            };
        }

        public async Task<ApplicationUser?> ValidateCredentialsAsync(string username, string password)
        {
            var usersFound = await userRepository.FindAsync(x => x.Username == username);

            var user = usersFound.FirstOrDefault();

            var isValidPassword = user is not null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return isValidPassword ? user : null;
        }

        public async Task<ForgotPasswordResult> ForgotPasswordAsync(string email)
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

            if (!user.EmailConfirmed)
            {
                return new ForgotPasswordResult
                {
                    Successful = false,
                    ErrorMessage = "You must confirm your email before you log in."
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

            var changePasswordUrl = $"{httpContextService.GetBaseUrl()}/account/change_password?code={user.PasswordResetToken}";

            try
            {
                await emailSender.SendEmailAsync(
                            user.Email,
                            $"Забыл пароль? Не проблема, {user.Username}.",
                            $$"""
                            <html>
                            <head>
                                <style>
                                    body {
                                        font-family: Arial, sans-serif;
                                        background-color: #f0f0f0;
                                    }
                                    h1 {
                                        color: #333333;
                                        text-align: center;
                                    }
                                    p {
                                        color: #555555;
                                        margin: 10px;
                                    }
                                    a {
                                        color: #0066cc;
                                        text-decoration: none;
                                    }
                                    a:hover {
                                        text-decoration: underline;
                                    }
                                </style>
                            </head>
                            <body>
                                <h1>Давно не виделись, чел!</h1>
                                <p>Перейди по ссылке чтобы поменять его: <a href="{{changePasswordUrl}}">Кликай сюды</a></p>
                                <p>P.S. Больше не забывай его, а то я сижу тут в подвале и работаю, тут сыро, и мокро и из еды только кошачий корм...
                                О нет, Фарис идёт... Забудь о том что ты только что прочитал.</p>
                            </body>
                            </html>
                            
                            """
                        );
            }
            catch(Exception ex) when(ex is SmtpCommandException or ParseException)
            {
                return new ForgotPasswordResult
                {
                    Successful = false,
                    ErrorMessage = "Email doesn't exist."
                };
            }

            await userRepository.SaveAsync();

            return new ForgotPasswordResult
            {
                Successful = true
            };
        }

        public async Task<ChangePasswordResult> ChangePasswordAsync(string code, string newPassword)
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
                Successful = true,
                User = user
            };
        }


        private static string CreateRandomToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(VerificationTokenLength);
            return Convert.ToHexString(randomBytes);
        }
    }
}