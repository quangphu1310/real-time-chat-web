using real_time_chat_web.Models;

namespace real_time_chat_web.Repository.IRepository
{
    public interface IRoomsUserRepository
    {
        Task<RoomsUser> CreateRoomsUserAsync(RoomsUser entity);
        //Task<RoomsUser> UpdateRoomsUserAsync(RoomsUser entity);
        Task RemoveRoomsUserAsync(RoomsUser entity);
        Task SaveAsync();
    }
}
