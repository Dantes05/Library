using AutoMapper;
using Domain.Entities;
using LibraryAPI.Controllers;
using LibraryApp.DTOs;

namespace LibraryApp
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserForRegistrationDto, User>()
                .ForMember(u => u.UserName, opt => opt.MapFrom(x => x.Email));
        }
    }
}
