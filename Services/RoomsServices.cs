using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Repository.IRepository;
using real_time_chat_web.Services.IServices;

namespace real_time_chat_web.Services
{
    public class RoomsServices : IServicesRooms
    {
        private readonly IRoomsRepository _roomsRepository;
        public RoomsServices(IRoomsRepository roomsRepository)
        {
            _roomsRepository = roomsRepository;
        }
        public async Task<APIResponse> CreateRoomAsync(RoomsCreateDTO room)
        {
            var newRoom = new ApplicationRooms
            {
                RoomName = room.RoomName,
                Description = room.Description,
                CreatedBy = room.CreatedBy,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            await _roomsRepository.CreateAsync(newRoom);
            return new APIResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Result = newRoom
            };
        }

        public async Task<APIResponse> DeleteRoomAsync(int id)
        {
            var room = await _roomsRepository.GetAsync(n => n.IdRooms == id);
            if (room == null)
            {
                return new APIResponse
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new List<string> { "Room not found" }
                };
            }
            await _roomsRepository.RemoveAsync(room);
            return new APIResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true
            };
        }

        public async Task<APIResponse> GetRoomAsync(int id)
        {
            var room = _roomsRepository.GetAsync(n => n.IdRooms == id);
            if(room == null)
            {

               return new APIResponse
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new List<string> { "Room not found" }
                };
            }
            
            return new APIResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Result = await _roomsRepository.GetAsync(n => n.IdRooms == id)
            };
        }

        public async Task<APIResponse> GetAllRoomsAsync()
        {
            await _roomsRepository.GetAllAsync();
            return new APIResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Result = await _roomsRepository.GetAllAsync()
            };
        }

        public async Task<APIResponse> UpdateRoomAsync(int id, RoomsUpdateDTO room)
        {
            var existingRoom = await _roomsRepository.GetAsync(r => r.IdRooms == id);
            if (existingRoom == null)
            {
                return new APIResponse { IsSuccess = false, Errors = new List<string> { "Room not found." } };
            }

            existingRoom.RoomName = room.RoomName;
            existingRoom.Description = room.Description;
            existingRoom.IsActive = room.IsActive;

            var updatedRoom = await _roomsRepository.UpdateRoomsAsync(existingRoom);
            return new APIResponse { IsSuccess = true, Result = updatedRoom };

        }
    }
}
