using AutoMapper;
using ChatApp.Application.Dtos.Users;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ... các mapping khác ...
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore());
        }
    }
}