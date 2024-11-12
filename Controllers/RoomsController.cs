using Microsoft.AspNetCore.Mvc;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Services;
using real_time_chat_web.Services.IServices;

namespace real_time_chat_web.Controllers
{
    [Route("api/rooms")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IServicesRooms _servicesRooms;

        public RoomsController(IServicesRooms servicesRooms)
        {
            _servicesRooms = servicesRooms;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> getAllRooms()
        {
            var response = await _servicesRooms.GetAllRoomsAsync();
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> getRooms(int id)
        {
            var response = await _servicesRooms.GetRoomAsync(id);
            if (!response.IsSuccess)
            {
                return BadRequest();
            }
            return Ok(response);

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRooms(RoomsCreateDTO roomsCreateDTO)
        {
            var response = await _servicesRooms.CreateRoomAsync(roomsCreateDTO);
            if (!response.IsSuccess)
            {
                return BadRequest();
            }
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteRooms(int id)
        {
            var response = await _servicesRooms.DeleteRoomAsync(id);
            if (!response.IsSuccess)
            {
                return BadRequest();
            }
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RoomsUpdateDTO>> UpdateRoom(int id, [FromBody] RoomsUpdateDTO room)
        {
            if (id != room.IdRooms) return BadRequest();

            var updatedRoom = await _servicesRooms.UpdateRoomAsync(id , room);
            if (updatedRoom == null) return NotFound();

            return Ok(updatedRoom);
        }
    }
}
