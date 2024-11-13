using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace real_time_chat_web.Models
{
    public class MessageReadStatus
    {
        [Key]
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string UserId { get; set; }
        public bool IsRead { get; set; } = false;

        [ForeignKey("MessageId")]
        public Messages Message { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
