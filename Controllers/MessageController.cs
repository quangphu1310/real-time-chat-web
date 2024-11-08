using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using real_time_chat_web.Data;
using real_time_chat_web.Models;

namespace real_time_chat_web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public MessagesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("room/{roomId}")]
        public async Task<ActionResult<IEnumerable<Messages>>> GetMessagesByRoom(int roomId)
        {
            return await _db.Messages
                .Where(m => m.RoomId == roomId)
                .Include(m => m.User)
                .ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Messages>> GetMessage(int id)
        {
            var message = await _db.Messages.FindAsync(id);

            if (message == null)
                return NotFound();

            return message;
        }

        [HttpPost]
        public async Task<ActionResult<Messages>> CreateMessage(Messages message)
        {
            message.SentAt = DateTime.UtcNow;
            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessage), new { id = message.MessageId }, message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(int id, Messages message)
        {
            if (id != message.MessageId)
                return BadRequest();

            _db.Entry(message).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MessageExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _db.Messages.FindAsync(id);
            if (message == null)
                return NotFound();

            _db.Messages.Remove(message);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool MessageExists(int id)
        {
            return _db.Messages.Any(e => e.MessageId == id);
        }
        [HttpPost("markAsRead/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId, string userId)
        {
            // Nếu dùng `MessageReadStatus`
            var status = await _db.MessageReadStatuses
                            .FirstOrDefaultAsync(m => m.MessageId == messageId && m.UserId == userId);

            if (status == null)
            {
                status = new MessageReadStatus
                {
                    MessageId = messageId,
                    UserId = userId,
                    IsRead = true
                };
                _db.MessageReadStatuses.Add(status);
            }
            else
            {
                status.IsRead = true;
                _db.MessageReadStatuses.Update(status);
            }

            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
