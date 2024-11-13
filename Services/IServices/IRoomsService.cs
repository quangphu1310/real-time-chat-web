using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;

namespace real_time_chat_web.Services.IServices
{
    public interface IRoomsService
    {
        Task<APIResponse> GetAllRoomsAsync();
        Task<APIResponse> GetRoomAsync(int id);
        Task<APIResponse> CreateRoomAsync(RoomsCreateDTO room);
        Task<APIResponse> UpdateRoomAsync(int id, RoomsUpdateDTO room);
        Task<APIResponse> DeleteRoomAsync(int id);
    }
}
