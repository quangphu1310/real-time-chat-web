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

            CreateMap<Rooms, RoomsUpdateDTO>().ReverseMap();
            CreateMap<Rooms, RoomsCreateDTO>().ReverseMap();
            CreateMap<RoomsUser, RoomsUserCreateDTO>().ReverseMap();
            CreateMap<RoomsUser, RoomsUserDTO>()
            .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Rooms.RoomName))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.PerUserName, opt => opt.MapFrom(src => src.PerUser.Name));

            CreateMap<VideoCall, VideoCallCreateDTO>().ReverseMap();
            CreateMap<VideoCallCreateDTO, VideoCall>().ReverseMap();

        }
    }
}
