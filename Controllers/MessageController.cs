using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;
using System;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using real_time_chat_web.Hubs;

[Route("api/[controller]")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public MessageController(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // Lấy danh sách tin nhắn của một phòng
    [HttpGet("room/{roomId}")]
    public async Task<ActionResult<IEnumerable<Messages>>> GetMessages(int roomId)
    {
        var messages = await _context.Messages
            .Where(m => m.RoomId == roomId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return Ok(messages);
    }

    // Gửi tin nhắn mới và lưu vào database
    [HttpPost("send")]
    public async Task<ActionResult<Messages>> SendMessage([FromBody] MessageCreateDTO messageDto)
    {
        var message = new Messages
        {
            Content = messageDto.Content,
            SentAt = DateTime.Now,
            UserId = messageDto.UserId,
            RoomId = messageDto.RoomId,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Thông báo tin nhắn mới cho các người dùng trong phòng thông qua SignalR
        await _hubContext.Clients.Group(message.RoomId.ToString()).SendAsync("ReceiveMessage", messageDto.UserId, messageDto.Content);

        return CreatedAtAction("GetMessages", new { roomId = message.RoomId }, message);
    }

    // Ghim tin nhắn
    [HttpPost("pin/{messageId}")]
    public async Task<ActionResult<Messages>> PinMessage(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
        {
            return NotFound();
        }

        message.IsPinned = true;
        await _context.SaveChangesAsync();

        return Ok(message);
    }

    // Xóa tin nhắn
    [HttpDelete("delete/{messageId}")]
    public async Task<ActionResult> DeleteMessage(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
        {
            return NotFound();
        }

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
