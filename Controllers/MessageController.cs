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
using real_time_chat_web.Migrations;
using System.Net;
using AutoMapper;

[Route("api/[controller]")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IMapper _mapper;
    private APIResponse _apiResponse;


    public MessageController(ApplicationDbContext context, IHubContext<ChatHub> hubContext, IMapper mapper)
    {
        _context = context;
        _hubContext = hubContext;
        _mapper = mapper;
        _apiResponse = new APIResponse();
    }

    // Lấy danh sách tin nhắn của một phòng
    [HttpGet("room/{roomId}")]
    //public async Task<ActionResult<IEnumerable<Messages>>> GetMessages(int roomId)
    //{
    //    var messages = await _context.Messages
    //        .Where(m => m.RoomId == roomId)
    //        .OrderBy(m => m.SentAt)
    //        .ToListAsync();

    //    return Ok(messages);
    //}
    public async Task<ActionResult<APIResponse>> GetMessagesByRoom(int roomId)
    {
        try
        {
            var messages = await _context.Messages
                .Where(m => m.RoomId == roomId)
                .Include(m => m.User)
                .ToListAsync();

            if (messages == null || messages.Count == 0)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add("No messages found for this room");
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_apiResponse);
            }

            var messageDtos = _mapper.Map<IEnumerable<MessageGetDTO>>(messages);
            _apiResponse.IsSuccess = true;
            _apiResponse.Result = messageDtos;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }
        catch (Exception ex)
        {
            _apiResponse.IsSuccess = false;
            _apiResponse.Errors.Add($"Error fetching messages: {ex.Message}");
            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
        }
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
            IsRead = false,
            FileUrl = ""

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
