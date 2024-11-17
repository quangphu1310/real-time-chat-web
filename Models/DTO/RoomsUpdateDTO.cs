using System.ComponentModel.DataAnnotations;

namespace real_time_chat_web.Models.DTO
{
    public class RoomsUpdateDTO
    {
        [Required]
        public int IdRooms { get; set; }
        [Required]
        public string RoomName { get; set; }
        public string Description { get; set; }
        [Required]
        public bool IsActive { get; set; }
    }
}
