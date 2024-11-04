namespace real_time_chat_web.Models.DTO
{
    public class UserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ConfirmationMessage { get; set; } // Thêm thuộc tính này để chứa mã xác nhận email
    }

}
