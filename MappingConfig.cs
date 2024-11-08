﻿using AutoMapper;
using real_time_chat_web.Models;
using real_time_chat_web.Models.DTO;

namespace real_time_chat_web
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<ApplicationUser, UserDTO>().ReverseMap();
            CreateMap<Messages, MessageCreateDTO>().ReverseMap();
        }
    }
}
