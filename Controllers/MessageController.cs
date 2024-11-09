using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System;

namespace real_time_chat_web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private APIResponse _apiResponse;

        public MessagesController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _apiResponse = new APIResponse();
        }

        // 1. Get messages by room
        [HttpGet("room/{roomId}")]
        public async Task<ActionResult<APIResponse>> GetMessagesByRoom(int roomId)
        {
            var messages = await _db.Messages
                .Where(m => m.RoomId == roomId)
                .Include(m => m.User)
                .ToListAsync();

            var messageDtos = _mapper.Map<IEnumerable<MessageGetDTO>>(messages);
            _apiResponse.IsSuccess = true;
            _apiResponse.Result = messageDtos;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }

        // 2. Get a single message by id
        [HttpGet("{id}")]
        public async Task<ActionResult<APIResponse>> GetMessage(int id)
        {
            var message = await _db.Messages
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.MessageId == id);

            if (message == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add("Message not found");
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_apiResponse);
            }

            var messageDto = _mapper.Map<MessageGetDTO>(message);
            _apiResponse.IsSuccess = true;
            _apiResponse.Result = messageDto;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }

        // 3. Create a new message
        [HttpPost]
        public async Task<ActionResult<APIResponse>> CreateMessage(MessageCreateDTO messagedto)
        {
            messagedto.SentAt = DateTime.UtcNow;

            var message = _mapper.Map<Messages>(messagedto);
            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            _apiResponse.IsSuccess = true;
            _apiResponse.Result = _mapper.Map<MessageGetDTO>(message);
            _apiResponse.StatusCode = HttpStatusCode.Created;
            return CreatedAtAction(nameof(GetMessage), new { id = message.MessageId }, _apiResponse);
        }

        // 4. Update a message
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(int id, MessageUpdateDTO messageUpdate)
        {
            if (id != messageUpdate.MessageId)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add("Message ID mismatch");
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_apiResponse);
            }

            var message = await _db.Messages.FindAsync(id);
            if (message == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add("Message not found");
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_apiResponse);
            }

            _mapper.Map(messageUpdate, message);
            _db.Entry(message).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
                _apiResponse.IsSuccess = true;
                _apiResponse.Result = _mapper.Map<MessageGetDTO>(message);
                _apiResponse.StatusCode = HttpStatusCode.OK;
            }
            catch (DbUpdateConcurrencyException)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add("Failed to update message due to concurrency issue");
                _apiResponse.StatusCode = HttpStatusCode.Conflict;
                return Conflict(_apiResponse);
            }

            return Ok(_apiResponse);
        }

        // 5. Delete a message
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> DeleteMessage(int id)
        {
            var message = await _db.Messages.FindAsync(id);
            if (message == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add("Message not found");
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_apiResponse);
            }

            _db.Messages.Remove(message);
            await _db.SaveChangesAsync();

            _apiResponse.IsSuccess = true;
            _apiResponse.StatusCode = HttpStatusCode.NoContent;
            return Ok(_apiResponse);
        }

        // 6. Mark a message as read
        [HttpPost("markAsRead/{messageId}")]
        public async Task<ActionResult<APIResponse>> MarkAsRead(int messageId, string userId)
        {
            var message = await _db.Messages.FirstOrDefaultAsync(m => m.MessageId == messageId);
            if (message == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add("Message not found");
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_apiResponse);
            }

            // Update IsRead to true
            message.IsRead = true;
            _db.Messages.Update(message);
            await _db.SaveChangesAsync();

            _apiResponse.IsSuccess = true;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }

        // 7. Mark a message as unread
        [HttpPost("markAsUnread/{messageId}")]
        public async Task<ActionResult<APIResponse>> MarkAsUnread(int messageId, string userId)
        {
            var message = await _db.Messages.FirstOrDefaultAsync(m => m.MessageId == messageId);
            if (message == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add("Message not found");
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_apiResponse);
            }

            // Update IsRead to false
            message.IsRead = false;
            _db.Messages.Update(message);
            await _db.SaveChangesAsync();

            _apiResponse.IsSuccess = true;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }

        // 8. Get the latest message in a room
        [HttpGet("latestMessage/{roomId}")]
        public async Task<ActionResult<APIResponse>> GetLatestMessageByRoom(int roomId)
        {
            var message = await _db.Messages
                .Where(m => m.RoomId == roomId)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            if (message == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add("No messages found");
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_apiResponse);
            }

            var messageDto = _mapper.Map<MessageGetDTO>(message);
            _apiResponse.IsSuccess = true;
            _apiResponse.Result = messageDto;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }

        // 9. Get rooms with unread messages for a user
        [HttpGet("unreadMessages/{userId}")]
        public async Task<ActionResult<APIResponse>> GetRoomsWithUnreadMessages(string userId)
        {
            var unreadRoomIds = await _db.MessageReadStatuses
                .Where(m => m.UserId == userId && !m.IsRead)
                .Select(m => m.Message.RoomId)
                .Distinct()
                .ToListAsync();

            _apiResponse.IsSuccess = true;
            _apiResponse.Result = unreadRoomIds;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }
    }
}
