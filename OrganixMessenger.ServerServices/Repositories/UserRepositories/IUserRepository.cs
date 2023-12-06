using OrganixMessenger.ServerModels.MessengerEntityModels.ApplicationUserModel;

namespace OrganixMessenger.ServerServices.Repositories.UserRepositories
{
    public interface IUserRepository : IRepository<ApplicationUser, Guid>
    {

    }
}
