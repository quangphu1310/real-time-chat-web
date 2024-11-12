using Microsoft.AspNetCore.Identity;

namespace real_time_chat_web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
    }
}
