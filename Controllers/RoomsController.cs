using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Services;
using real_time_chat_web.Services.IServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;

namespace real_time_chat_web.Controllers
{
    [Route("api/rooms")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomsService _roomsServices;
        private readonly UserManager<ApplicationUser> _userManager;
        public RoomsController(IRoomsService roomsService, UserManager<ApplicationUser> userManager)
        {
            _roomsServices = roomsService;
            _userManager = userManager;
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
        [Authorize(Roles = "admin", AuthenticationSchemes = "Bearer")]

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
    }
}
