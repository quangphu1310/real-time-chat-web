using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using real_time_chat_web.Hubs;

namespace real_time_chat_web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoCallController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public VideoCallController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartVideoCall([FromBody] VideoCallRequest request)
        {
            await _hubContext.Clients.Group(request.RoomId).SendAsync("ReceiveVideoCall", request.RoomId, request.CallerName);
            return Ok(new { Message = "Notification sent successfully." });
        }
    }

    public class VideoCallRequest
    {
        public string RoomId { get; set; }
        public string CallerName { get; set; }
    }
}
