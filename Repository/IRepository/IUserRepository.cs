using real_time_chat_web.Models;

namespace real_time_chat_web.Repository.IRepository
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<ApplicationUser> UpdateAsync(ApplicationUser entity);
        Task<ApplicationUser> CreateAsync(ApplicationUser entity);
    }
}
