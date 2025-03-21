using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace LibraryApp
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Author, AuthorDto>();
            CreateMap<AuthorDto, Author>();
            CreateMap<CreateAuthorDto, Author>();
            CreateMap<Author, CreateAuthorDto>();

            CreateMap<BookCreateDto, Book>()
                .ForMember(dest => dest.ImagePath, opt => opt.Ignore());
            CreateMap<BookUpdateDto, Book>()
                .ForMember(dest => dest.ImagePath, opt => opt.Ignore());
            CreateMap<Book, BookDto>()
             .ForMember(dest => dest.AuthorName, opt => opt.Ignore()) 
             .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImagePath)); 

            CreateMap<BookRental, BookRentalDto>();
            CreateMap<BookRentalDto, BookRental>();

            CreateMap<UserForRegistrationDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));


            CreateMap<UserForRegistrationDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            CreateMap<User, AuthResponseDto>();
        }
    }
}