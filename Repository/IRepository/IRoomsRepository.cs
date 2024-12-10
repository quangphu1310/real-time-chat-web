using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;

namespace real_time_chat_web.Repository.IRepository
{
    public interface IRoomsRepository : IRepository<Rooms>
    {
        Task<Rooms> UpdateRoomsAsync(Rooms entity);
        Task<Rooms> CreateRoomsAsync(Rooms entity);
        Task<MessageGetDTO> GetLastMessageAsync(int idRooms);
    }
}
