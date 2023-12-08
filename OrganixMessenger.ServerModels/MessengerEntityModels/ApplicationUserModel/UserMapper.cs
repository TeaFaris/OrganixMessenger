namespace OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationUserModel
{
    public static class UserMapper
    {
        public static UserDTO ToDTO(this ApplicationUser user)
        {
            return new UserDTO
            {
                Email = user.Email,
                Id = user.Id,
                LastOnline = user.LastOnline,
                ProfilePicture = user.ProfilePicture?.ToDTO(),
                Role = user.Role,
                Username = user.Name
            };
        }
    }
}
