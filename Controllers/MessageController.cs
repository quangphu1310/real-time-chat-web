using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MessagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Send a message
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SendMessage(MessageCreateDTO model)
    {
        var message = new Messages
        {
            Content = model.Content,
            FileUrl = model.FileUrl,
            IsPinned = false,
            SentAt = DateTime.UtcNow,
            UserId = model.UserId,
            RoomId = model.RoomId
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Ok(message);
    }

    // 2. Get all messages for a room
    [HttpGet("room/{roomId}")]
    [Authorize]
    public async Task<IActionResult> GetMessages(int roomId)
    {
        var messages = await _context.Messages
            .Where(m => m.RoomId == roomId)
            .Include(m => m.User)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return Ok(messages);
    }

    // 3. Pin/Unpin a message (Moderate User only)
    [HttpPatch("{messageId}/pin")]
    [Authorize(Policy = "ModerateUserPolicy")]
    public async Task<IActionResult> PinMessage(int messageId, [FromBody] MessageGetDTO model)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null) return NotFound("Message not found");

        message.IsPinned = model.IsPinned;
        await _context.SaveChangesAsync();

        return Ok(message);
    }

    // 4. Delete a message (Moderate User only)
    [HttpDelete("{messageId}")]
    [Authorize(Policy = "ModerateUserPolicy")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null) return NotFound("Message not found");

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return Ok("Message deleted successfully");
    }
}



//using AutoMapper;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using real_time_chat_web.Data;
//using real_time_chat_web.Models.DTO;
//using real_time_chat_web.Models;
//using System.Net;
//using Microsoft.EntityFrameworkCore;

//[ApiController]
//[Route("api/[controller]")]
//public class MessagesController : ControllerBase
//{
//    private readonly ApplicationDbContext _db;
//    private readonly IMapper _mapper;
//    private APIResponse _apiResponse;

//    public MessagesController(ApplicationDbContext db, IMapper mapper)
//    {
//        _db = db;
//        _mapper = mapper;
//        _apiResponse = new APIResponse();
//    }
//    [HttpGet("{id}")]
//    //[Authorize( AuthenticationSchemes = "Bearer")]
//    public async Task<ActionResult<APIResponse>> GetMessage(int id)
//    {
//        try
//        {
//            var message = await _db.Messages
//                .Include(m => m.User)
//                .FirstOrDefaultAsync(m => m.MessageId == id);

//            if (message == null)
//            {
//                _apiResponse.IsSuccess = false;
//                _apiResponse.Errors.Add("Message not found");
//                _apiResponse.StatusCode = HttpStatusCode.NotFound;
//                return NotFound(_apiResponse);
//            }

//            var messageDto = _mapper.Map<MessageGetDTO>(message);
//            _apiResponse.IsSuccess = true;
//            _apiResponse.Result = messageDto;
//            _apiResponse.StatusCode = HttpStatusCode.OK;
//            return Ok(_apiResponse);
//        }
//        catch (Exception ex)
//        {
//            _apiResponse.IsSuccess = false;
//            _apiResponse.Errors.Add($"Error fetching message: {ex.Message}");
//            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
//            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
//        }
//    }
//    //Get messages by room
//   [HttpGet("room/{roomId}")]
//    //[Authorize( AuthenticationSchemes = "Bearer")]
//    public async Task<ActionResult<APIResponse>> GetMessagesByRoom(int roomId)
//    {
//        try
//        {
//            var messages = await _db.Messages
//                .Where(m => m.RoomId == roomId)
//                .Include(m => m.User)
//                .ToListAsync();

//            if (messages == null || messages.Count == 0)
//            {
//                _apiResponse.IsSuccess = false;
//                _apiResponse.Errors.Add("No messages found for this room");
//                _apiResponse.StatusCode = HttpStatusCode.NotFound;
//                return NotFound(_apiResponse);
//            }

//            var messageDtos = _mapper.Map<IEnumerable<MessageGetDTO>>(messages);
//            _apiResponse.IsSuccess = true;
//            _apiResponse.Result = messageDtos;
//            _apiResponse.StatusCode = HttpStatusCode.OK;
//            return Ok(_apiResponse);
//        }
//        catch (Exception ex)
//        {
//            _apiResponse.IsSuccess = false;
//            _apiResponse.Errors.Add($"Error fetching messages: {ex.Message}");
//            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
//            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
//        }
//    }

//    //Send a message
//   [HttpPost]
//    //[Authorize(AuthenticationSchemes = "Bearer")]
//    public async Task<IActionResult> SendMessage([FromBody] MessageCreateDTO messageDto)
//    {
//        try
//        {
//            messageDto.SentAt = DateTime.UtcNow;

//            var message = _mapper.Map<Messages>(messageDto);
//            message.IsPinned = false;

//            _db.Messages.Add(message);
//            await _db.SaveChangesAsync();

//            _apiResponse.IsSuccess = true;
//            _apiResponse.Result = _mapper.Map<MessageGetDTO>(message);
//            _apiResponse.StatusCode = HttpStatusCode.Created;
//            return CreatedAtAction(nameof(GetMessage), new { id = message.MessageId }, _apiResponse);
//        }
//        catch (Exception ex)
//        {
//            _apiResponse.IsSuccess = false;
//            _apiResponse.Errors.Add($"Error creating message: {ex.Message}");
//            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
//            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
//        }
//    }

//    //Pin or Unpin a message
//   [HttpPut("pin/{messageId}")]
//    //[Authorize(AuthenticationSchemes = "Bearer")]
//    public async Task<IActionResult> PinMessage(int messageId, [FromQuery] bool isPinned)
//    {
//        try
//        {
//            var message = await _db.Messages.FindAsync(messageId);
//            if (message == null)
//            {
//                _apiResponse.IsSuccess = false;
//                _apiResponse.Errors.Add("Message not found");
//                _apiResponse.StatusCode = HttpStatusCode.NotFound;
//                return NotFound(_apiResponse);
//            }

//            message.IsPinned = isPinned;
//            await _db.SaveChangesAsync();

//            _apiResponse.IsSuccess = true;
//            _apiResponse.StatusCode = HttpStatusCode.OK;
//            return Ok(_apiResponse);
//        }
//        catch (Exception ex)
//        {
//            _apiResponse.IsSuccess = false;
//            _apiResponse.Errors.Add($"Error pinning message: {ex.Message}");
//            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
//            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
//        }
//    }

//    //Mark a message as read
//   [HttpPut("read/{messageId}")]
//    //[Authorize(AuthenticationSchemes = "Bearer")]
//    public async Task<IActionResult> MarkAsRead(int messageId)
//    {
//        try
//        {
//            var message = await _db.Messages.FindAsync(messageId);
//            if (message == null)
//            {
//                _apiResponse.IsSuccess = false;
//                _apiResponse.Errors.Add("Message not found");
//                _apiResponse.StatusCode = HttpStatusCode.NotFound;
//                return NotFound(_apiResponse);
//            }

//            message.IsRead = true;
//            await _db.SaveChangesAsync();

//            _apiResponse.IsSuccess = true;
//            _apiResponse.StatusCode = HttpStatusCode.OK;
//            return Ok(_apiResponse);
//        }
//        catch (Exception ex)
//        {
//            _apiResponse.IsSuccess = false;
//            _apiResponse.Errors.Add($"Error marking message as read: {ex.Message}");
//            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
//            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
//        }
//    }

//    //Delete a message(for moderators and admins only)
//    [HttpDelete("{messageId}")]
//    [Authorize(Roles = "admin,mod", AuthenticationSchemes = "Bearer")]
//    public async Task<IActionResult> DeleteMessage(int messageId)
//    {
//        try
//        {
//            var message = await _db.Messages.FindAsync(messageId);
//            if (message == null)
//            {
//                _apiResponse.IsSuccess = false;
//                _apiResponse.Errors.Add("Message not found");
//                _apiResponse.StatusCode = HttpStatusCode.NotFound;
//                return NotFound(_apiResponse);
//            }

//            _db.Messages.Remove(message);
//            await _db.SaveChangesAsync();

//            _apiResponse.IsSuccess = true;
//            _apiResponse.StatusCode = HttpStatusCode.OK;
//            return Ok(_apiResponse);
//        }
//        catch (Exception ex)
//        {
//            _apiResponse.IsSuccess = false;
//            _apiResponse.Errors.Add($"Error deleting message: {ex.Message}");
//            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
//            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
//        }
//    }
//}

