using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Services;
using real_time_chat_web.Services.IServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using real_time_chat_web.Repository.IRepository;
using AutoMapper;

namespace real_time_chat_web.Controllers
{
    [Route("api/rooms")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomsService _roomsServices;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRoomsUserRepository _roomsUserRepository;
        private readonly IVideoCallService _videoCallService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        public RoomsController(IRoomsService roomsService, UserManager<ApplicationUser> userManager, IRoomsUserRepository roomsUserRepository, IVideoCallService videoCallService, INotificationService notificationService, IMapper mapper)
        {
            _roomsServices = roomsService;
            _userManager = userManager;
            _roomsUserRepository = roomsUserRepository;
            _videoCallService = videoCallService;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles ="admin", AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> getAllRooms()
        {
            var response = await _roomsServices.GetAllRoomsAsync();
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Bearer")]

        public async Task<IActionResult> getRooms(int id)
        {
            var response = await _roomsServices.GetRoomAsync(id);
            if (!response.IsSuccess)
            {
                new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new List<string> { "Room not found" }
                };
            }
            return Ok(response);

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "admin, mod", AuthenticationSchemes = "Bearer")]

        public async Task<IActionResult> CreateRooms(RoomsCreateDTO roomsCreateDTO)
        {
            var user = await _userManager.GetUserAsync(User);
            roomsCreateDTO.CreatedBy = user.Id;
            var response = await _roomsServices.CreateRoomAsync(roomsCreateDTO);
            
            if (!response.IsSuccess)
            {
                return BadRequest();
            }
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "admin, mod", AuthenticationSchemes = "Bearer")]

        public async Task<IActionResult> DeleteRooms(int id)
        {
            var response = await _roomsServices.DeleteRoomAsync(id);
            if (!response.IsSuccess)
            {
                return BadRequest();
            }
            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "admin", AuthenticationSchemes = "Bearer")]

        public async Task<ActionResult<RoomsUpdateDTO>> UpdateRoom(int id, [FromBody] RoomsUpdateDTO room)
        {
            if (id != room.IdRooms) 
                return NotFound();

            var updatedRoom = await _roomsServices.UpdateRoomAsync(id , room);
            if (updatedRoom == null) 
                return BadRequest();

            return Ok(updatedRoom);
        }



        //start video call jitsi
        [HttpPost("{RoomId}/start-video-call")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> StartVideoCall(int RoomId)
        {
            // Kiểm tra phòng chat tồn tại
            var room = await _roomsServices.GetRoomAsync(RoomId);
            if (!room.IsSuccess)
            {
                return NotFound(new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new List<string> { "Room not found" }
                });
            }

            // Tạo tên phòng Jitsi dựa trên IdRooms
            string roomName = $"room_{RoomId}_{Guid.NewGuid()}";
            string videoCallUrl = $"https://meet.jit.si/{roomName}";

            // Gửi thông báo tới các thành viên trong phòng
            var members = await _roomsUserRepository.GetRoomsUserAsync(RoomId);
            foreach (var member in members)
            {
                await _notificationService.NotifyUser(member.Id, new
                {
                    RoomId = RoomId,
                    VideoCallUrl = videoCallUrl,
                    Message = $"{User.Identity.Name} đã bắt đầu một cuộc gọi video trong phòng."
                });
            }
            var user = await _userManager.GetUserAsync(User);
            // Lưu thông tin cuộc gọi video (nếu cần)
            var videoCall = new VideoCallCreateDTO
            {
                RoomId = RoomId,
                VideoCallUrl = videoCallUrl,
                CreatedBy = user.Id,
                CreatedAt = DateTime.UtcNow,
                Status = "Ongoing"
            };
            VideoCall video = _mapper.Map<VideoCall>(videoCall);
            await _videoCallService.CreateVideoCallAsync(video);

            return Ok(new { VideoCallUrl = videoCallUrl });
        }


        // Get video call
        [HttpGet("{roomId}/current-video-call")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetCurrentVideoCall(int roomId)
        {
            var videoCall = await _videoCallService.GetCurrentVideoCallAsync(roomId);
            if (videoCall == null || videoCall.Status != "Ongoing")
            {
                return NotFound(new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new List<string> { "No active video call in this room." }
                });
            }
            return Ok(new { VideoCallUrl = videoCall.VideoCallUrl });
        }


    }
}
