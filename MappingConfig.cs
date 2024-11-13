using AutoMapper;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;

namespace real_time_chat_web
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<ApplicationUser, UserDTO>().ReverseMap();


            CreateMap<Messages, MessageGetIdRoomDTO>().ReverseMap();

            CreateMap<Messages, MessageGetDTO>().ReverseMap();

            CreateMap<Messages, MessageReadStatusDTO>().ReverseMap();


            CreateMap<Messages, MessageCreateDTO>().ReverseMap();

            CreateMap<Messages, MessageUpdateDTO>().ReverseMap();

            CreateMap<ApplicationUser, ApplicationUserDTO>().ReverseMap();
            CreateMap<ApplicationUser, ApplicationUserCreateDTO>().ReverseMap();
            CreateMap<ApplicationUser, ApplicationUserUpdateDTO>().ReverseMap();
            CreateMap<ApplicationUser, ApplicationUserProfileDTO>().ReverseMap();

        }
    }
}
