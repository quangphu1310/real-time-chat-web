using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace real_time_chat_web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext db;

        public ChatHub(ApplicationDbContext _db)
        {
            db = _db;
        }

        // Gửi tin nhắn đến một room
        public async Task SendMessage(int roomId, string userId, string content, string fileUrl = null)
        {
            var message = new Messages
            {
                RoomId = roomId,
                UserId = userId,
                Content = content,
                FileUrl = fileUrl,
                SentAt = DateTime.UtcNow,
                IsPinned = false
            };

            db.Messages.Add(message);
            await db.SaveChangesAsync();

            // Phát tin nhắn tới tất cả các client trong room
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", new
            {
                MessageId = message.MessageId,
                UserId = message.UserId,
                Content = message.Content,
                FileUrl = message.FileUrl,
                SentAt = message.SentAt,
                IsRead = false // mặc định chưa đọc
            });
        }

        // Tham gia room (group) với RoomId
        public async Task JoinRoom(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        }

        // Rời room
        public async Task LeaveRoom(int roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
        }

        // Đánh dấu tin nhắn là đã đọc
        public async Task MarkMessageAsRead(int messageId, string userId)
        {
            var status = await db.MessageReadStatuses
                .FirstOrDefaultAsync(m => m.MessageId == messageId && m.UserId == userId);

            if (status == null)
            {
                // Nếu chưa có trạng thái đọc, thêm mới
                status = new MessageReadStatus
                {
                    MessageId = messageId,
                    UserId = userId,
                    IsRead = true
                };
                db.MessageReadStatuses.Add(status);
            }
            else
            {
                // Nếu đã có trạng thái đọc, cập nhật
                status.IsRead = true;
                db.MessageReadStatuses.Update(status);
            }

            await db.SaveChangesAsync();

            // Gửi thông báo trạng thái đọc đến các client trong room
            await Clients.Group(status.Message.RoomId.ToString()).SendAsync("MessageRead", messageId, userId);
        }

    }

}
