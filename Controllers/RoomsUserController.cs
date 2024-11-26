using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Repository;
using real_time_chat_web.Repository.IRepository;
using System.Net;

namespace real_time_chat_web.Controllers
{
    [Route("api/Rooms-User")]
    [ApiController]
    
    public class RoomsUserController : ControllerBase
    {
        private readonly IRoomsUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly APIResponse _apiResponse;
        public RoomsUserController(IRoomsUserRepository repository, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _mapper = mapper;
            _userManager = userManager;
            _apiResponse = new APIResponse();
        }

        [HttpPost("add-user-in-room")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "mod", AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> CreateRoomsUser([FromBody] RoomsUserCreateDTO CreRoomUser)
        {
            
                if(CreRoomUser == null || CreRoomUser.IdUser == null || CreRoomUser.IdUser.Count == 0 || CreRoomUser.IdRooms == null)
                {
                    return BadRequest("UserId is Null");
                }
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("Invalid User");
                }
                
                CreRoomUser.IdPerAdd = user.Id;
               
                foreach (var item in CreRoomUser.IdUser)
                {
                var userExists = await _userManager.FindByIdAsync(item);
                if (userExists == null)
                {
                    return BadRequest($"User with ID {item} does not exist.");
                }
                //var NewUser = _mapper.Map<RoomsUser>(CreRoomUser);
                    var newUser = new RoomsUser
                    {
                        IdRooms = CreRoomUser.IdRooms,
                        IdUser = item, // Lưu từng `IdUser`
                        IdPerAdd = CreRoomUser.IdPerAdd,
                        DayAdd = DateTime.Now
                    };
                    await _repository.CreateRoomsUserAsync(newUser);
                }
                _apiResponse.IsSuccess = true;
                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.Result = CreRoomUser;
                return Ok(_apiResponse);
        }

        [HttpDelete("remove-user-out-room")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "mod", AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> DeleteRoomsUser([FromBody] RoomsUserDeleteDTO DeleteRoomsUser)
        {
            if (DeleteRoomsUser == null || DeleteRoomsUser.IdUser == null || DeleteRoomsUser.IdUser.Count == 0 || DeleteRoomsUser.IdRooms <= 0)
            {
                return BadRequest("Invalid Data");
            }
            foreach(var item in DeleteRoomsUser.IdUser)
            {
                var User = new RoomsUser
                {
                    IdUser = item,
                    IdRooms = DeleteRoomsUser.IdRooms
                };

                await _repository.RemoveRoomsUserAsync(User);
            }
            _apiResponse.IsSuccess = true;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            _apiResponse.Result = DeleteRoomsUser;
            return Ok(_apiResponse);

        }

        [HttpGet("{IdRooms}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllUsersInRoom(int IdRooms)
        {
            var users = await _repository.GetRoomsUserAsync(IdRooms);

            if (users == null || users.Count == 0)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                _apiResponse.Errors.Add("No users found in the specified room.");
                return NotFound(_apiResponse);
            }

            _apiResponse.IsSuccess = true;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            _apiResponse.Result = users;
            return Ok(_apiResponse);
        }

        [HttpGet("rooms-by-user/{Name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "mod, admin", AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetRoomsByUser(string Name)
        {
            var rooms = await _repository.GetRoomsByUserAsync(Name);
            if (rooms == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                _apiResponse.Errors = new List<string>();
            }
            _apiResponse.IsSuccess = true;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            _apiResponse.Result = rooms;
            return Ok(_apiResponse);
        }
    }
}
