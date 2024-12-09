using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using real_time_chat_web.Data;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Models;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.IO;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private APIResponse _apiResponse;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public MessagesController(ApplicationDbContext db, IMapper mapper, IConfiguration configuration, IWebHostEnvironment env)
    {
        _db = db;
        _mapper = mapper;
        _apiResponse = new APIResponse();
        _env = env;
        _configuration= configuration;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<APIResponse>> GetMessage(int id)
    {
        try
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
        catch (Exception ex)
        {
            _apiResponse.IsSuccess = false;
            _apiResponse.Errors.Add($"Error fetching message: {ex.Message}");
            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] MessageCreateDTO messageDto)
    {
        try
        {
            messageDto.SentAt = DateTime.UtcNow;
            var message = _mapper.Map<Messages>(messageDto);
            message.IsPinned = false;

            // If a file URL is provided, associate it with the message
            if (!string.IsNullOrEmpty(messageDto.FileUrl))
            {
                message.FileUrl = messageDto.FileUrl;
            }

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            _apiResponse.IsSuccess = true;
            _apiResponse.Result = _mapper.Map<MessageGetDTO>(message);
            _apiResponse.StatusCode = HttpStatusCode.Created;

            return CreatedAtAction(nameof(GetMessage), new { id = message.MessageId }, _apiResponse);
        }
        catch (Exception ex)
        {
            _apiResponse.IsSuccess = false;
            _apiResponse.Errors.Add($"Error creating message: {ex.Message}");
            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
        }
    }

    [HttpGet("room/{roomId}")]
    //[Authorize( AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<APIResponse>> GetMessagesByRoom(int roomId)
    {
        try
        {
            var messages = await _db.Messages
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
    [HttpGet("room/{roomId}/pinned-messages")]
    //[Authorize( AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<APIResponse>> GetMessagePinnedInRoom(int roomId)
    {
        try
        {
            var messages = await _db.Messages
                .Where(m => m.RoomId == roomId && m.IsPinned)
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

    [HttpPost("upload-file/{RoomId}")]
    public async Task<IActionResult> UploadFile(int RoomId, IFormFile file, [FromForm] string UserId)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        if (string.IsNullOrWhiteSpace(UserId))
        {
            return BadRequest("Invalid UserId.");
        }

        try
        {
            var cloudinary = new Cloudinary(new Account(
                       cloud: _configuration.GetSection("Cloudinary:CloudName").Value,
                       apiKey: _configuration.GetSection("Cloudinary:ApiKey").Value,
                       apiSecret: _configuration.GetSection("Cloudinary:ApiSecret").Value
                   ));

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.OpenReadStream())
            };
            var uploadResult = cloudinary.Upload(uploadParams);
            string fileUrl = uploadResult.Url.ToString();

            string fileHtml = Path.GetExtension(file.FileName).ToLower() switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" =>
                    $"<a href=\"{fileUrl}\" target=\"_blank\"><img src=\"{fileUrl}\" class=\"post-image\"></a>",
                _ => $"<a href=\"{fileUrl}\" target=\"_blank\">[File]</a>"
            };
            var newMessage = new Messages
            {
                RoomId = RoomId,
                UserId = UserId,
                SentAt = DateTime.Now,
                FileUrl = fileUrl,
                Content = fileHtml, 
                IsPinned = false,
                IsRead = false
            };

            _db.Messages.Add(newMessage);
            await _db.SaveChangesAsync();
            return Ok(new { FileUrl = fileUrl, Content = fileHtml });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new APIResponse
            {
                IsSuccess = false,
                Errors = { $"Error uploading file: {ex.Message}" },
                StatusCode = HttpStatusCode.InternalServerError
            });
        }
    }

    [HttpPut("pin/{messageId}")]
    public async Task<IActionResult> PinMessage(int messageId)
        {
        try
        {
            var message = await _db.Messages.FindAsync(messageId);
            if (message == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add("Message not found");
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_apiResponse);
            }
            if(message.IsPinned == true)
            {
                message.IsPinned = false;
            }
            else message.IsPinned = true;
            await _db.SaveChangesAsync();

            _apiResponse.IsSuccess = true;
            _apiResponse.Result = message;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }
        catch (Exception ex)
        {
            _apiResponse.IsSuccess = false;
            _apiResponse.Errors.Add($"Error pinning message: {ex.Message}");
            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
        }
    }

    [HttpPut("read/{messageId}")]
    public async Task<IActionResult> MarkAsRead(int messageId)
    {
        try
        {
            var message = await _db.Messages.FindAsync(messageId);
            if (message == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add("Message not found");
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_apiResponse);
            }

            message.IsRead = true;
            await _db.SaveChangesAsync();

            _apiResponse.IsSuccess = true;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }
        catch (Exception ex)
        {
            _apiResponse.IsSuccess = false;
            _apiResponse.Errors.Add($"Error marking message as read: {ex.Message}");
            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
        }
    }

    [HttpDelete("{messageId}")]
    [Authorize(Roles = "admin,mod", AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        try
        {
            var message = await _db.Messages.FindAsync(messageId);
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
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(_apiResponse);
        }
        catch (Exception ex)
        {
            _apiResponse.IsSuccess = false;
            _apiResponse.Errors.Add($"Error deleting message: {ex.Message}");
            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
        }
    }

    //Get message and time cuối cùng
    [HttpGet("get-message-last/{roomId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //[Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> GetLastMessage(int roomId)
    {
        try
        {
            var lastMessage = await _db.Messages
                .Where(m => m.RoomId == roomId)
                .OrderByDescending(m => m.SentAt)
                .Select(m => new
                {
                    m.Content,
                    m.SentAt,
                    UserName = m.User.UserName, 
                    m.Room.RoomName
                })
                .FirstOrDefaultAsync();

                if (lastMessage == null)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.Errors.Add("No messages found in the specified room");
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_apiResponse);
            }

            _apiResponse.IsSuccess = true;
            _apiResponse.Result = lastMessage;
            _apiResponse.StatusCode = HttpStatusCode.OK;

            return Ok(_apiResponse);
        }
        catch (Exception ex)
        {
            _apiResponse.IsSuccess = false;
            _apiResponse.Errors.Add($"Error fetching last message: {ex.Message}");
            _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
            return StatusCode((int)HttpStatusCode.InternalServerError, _apiResponse);
        }
    }
}
