using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Repository;
using real_time_chat_web.Repository.IRepository;
using real_time_chat_web.Services.IServices;
using System.Net;

namespace real_time_chat_web.Services
{
    public class RoomsServices : IRoomsService
    {
        private readonly IRoomsRepository _roomsRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public RoomsServices(IRoomsRepository roomsRepository, IMapper mapper, UserManager<ApplicationUser> userManager )
        {
            _roomsRepository = roomsRepository;
            _mapper = mapper;
            _userManager = userManager;
        }
        
        public async Task<APIResponse> CreateRoomAsync(RoomsCreateDTO room)
        {
            var newRoom = _mapper.Map<Rooms>(room);

            await _roomsRepository.CreateRoomsAsync(newRoom);
            return new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
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
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new List<string> { "Room not found" }
                };
            }
            await _roomsRepository.RemoveAsync(room);
            return new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            };
        }

        public async Task<APIResponse> GetRoomAsync(int id)
        {
            var room = await _roomsRepository.GetAsync(n => n.IdRooms == id);
            if(room == null)
            {

               return new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new List<string> { "Room not found" }
                };
            }

            return new APIResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Result = room
            }; 
        }

        public async Task<APIResponse> GetAllRoomsAsync()
        {
            
            return new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = await _roomsRepository.GetAllAsync()
            };
        }

        public async Task<APIResponse> UpdateRoomAsync(int id, RoomsUpdateDTO room)
        {
            var existingRoom = await _roomsRepository.GetAsync(r => r.IdRooms == id);
            if (existingRoom == null)
            {
                return new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new List<string> { "Room not found." }
                };
            }

            existingRoom.RoomName = room.RoomName;
            existingRoom.Description = room.Description;
            existingRoom.IsActive = room.IsActive;

            var updatedRoom = await _roomsRepository.UpdateRoomsAsync(existingRoom);
            return new APIResponse 
            { 
                IsSuccess = true, 
                Result = updatedRoom 
            };

        }
    }
}
