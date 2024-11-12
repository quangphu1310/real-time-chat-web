using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;

namespace real_time_chat_web.Repository.IRepository
{
    public interface IRoomsRepository : IRepository<ApplicationRooms>
    {
        Task<ApplicationRooms> UpdateRoomsAsync(ApplicationRooms entity);
        
    }
}
