namespace real_time_chat_web.Models.DTO
{
    public class ApplicationUserProfileDTO
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? Image { get; set; }
    }
}
