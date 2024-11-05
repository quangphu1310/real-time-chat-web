namespace real_time_chat_web.Models.DTO
{
    public class RequestResetPasswordDTO
    {
        public string Token {  get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

    }
}
