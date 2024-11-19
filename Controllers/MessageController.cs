using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using real_time_chat_web.Data;
using real_time_chat_web.Models.DTO;
using real_time_chat_web.Models;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.IO;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private APIResponse _apiResponse;
    private readonly IWebHostEnvironment _env;

    public MessagesController(ApplicationDbContext db, IMapper mapper, IWebHostEnvironment env)
    {
        _db = db;
        _mapper = mapper;
        _apiResponse = new APIResponse();
        _env = env;
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
    //[HttpPost("upload-file/{RoomId}")]
    //public async Task<IActionResult> UploadFile(int RoomId, IFormFile file, [FromForm] string UserId)
    //{
    //    if (file == null || file.Length == 0)
    //    {
    //        return BadRequest("No file uploaded.");
    //    }

    //    if (string.IsNullOrWhiteSpace(UserId))
    //    {
    //        return BadRequest("Invalid UserId.");
    //    }

    //    try
    //    {
    //        // Tạo file name duy nhất
    //        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
    //        string directoryPath = Path.Combine(_env.WebRootPath, "Messages");

    //        // Đảm bảo thư mục tồn tại
    //        if (!Directory.Exists(directoryPath))
    //        {
    //            Directory.CreateDirectory(directoryPath);
    //        }

    //        string filePath = Path.Combine(directoryPath, fileName);

    //        // Lưu file vào server
    //        using (var fileStream = new FileStream(filePath, FileMode.Create))
    //        {
    //            await file.CopyToAsync(fileStream);
    //        }

    //        // Construct file URL
    //        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
    //        string fileUrl = $"{baseUrl}/Messages/{fileName}";

    //        // Tạo nội dung HTML cho thẻ <a> với <img> nếu là hình ảnh
    //        string fileHtml = Path.GetExtension(file.FileName).ToLower() switch
    //        {
    //            ".jpg" or ".jpeg" or ".png" or ".gif" =>
    //                $"<a href=\"{fileUrl}\" target=\"_blank\"><img src=\"{fileUrl}\" class=\"post-image\"></a>",
    //            _ => $"<a href=\"{fileUrl}\" target=\"_blank\">[File]</a>"
    //        };

    //        // Lưu tin nhắn kèm nội dung HTML vào cơ sở dữ liệu
    //        var newMessage = new Messages
    //        {
    //            RoomId = RoomId,
    //            UserId = UserId,
    //            SentAt = DateTime.Now,
    //            FileUrl = fileUrl,
    //            Content = fileHtml, // Lưu nội dung HTML vào Content
    //            IsPinned = false,
    //            IsRead = false
    //        };

    //        _db.Messages.Add(newMessage);
    //        await _db.SaveChangesAsync();

    //        return Ok(new { FileUrl = fileUrl, Content = fileHtml });
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode((int)HttpStatusCode.InternalServerError, new APIResponse
    //        {
    //            IsSuccess = false,
    //            Errors = { $"Error uploading file: {ex.Message}" },
    //            StatusCode = HttpStatusCode.InternalServerError
    //        });
    //    }
    //}
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
            // Create a unique file name
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string directoryPath = Path.Combine(_env.WebRootPath, "Messages");

            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, fileName);

            // Save the file to the server
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Construct file URL
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
            string fileUrl = $"{baseUrl}/Messages/{fileName}";

            // HTML content for images
            string fileHtml = Path.GetExtension(file.FileName).ToLower() switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" =>
                    $"<a href=\"{fileUrl}\" target=\"_blank\"><img src=\"{fileUrl}\" class=\"post-image\"></a>",
                _ => $"<a href=\"{fileUrl}\" target=\"_blank\">[File]</a>"
            };

            // Save message with file URL and HTML content in the database
            var newMessage = new Messages
            {
                RoomId = RoomId,
                UserId = UserId,
                SentAt = DateTime.Now,
                FileUrl = fileUrl,
                Content = fileHtml, // Save HTML content
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
    public async Task<IActionResult> PinMessage(int messageId, [FromQuery] bool isPinned)
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

            message.IsPinned = isPinned;
            await _db.SaveChangesAsync();

            _apiResponse.IsSuccess = true;
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
}
