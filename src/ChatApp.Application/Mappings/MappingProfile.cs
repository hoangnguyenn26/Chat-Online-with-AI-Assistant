using AutoMapper;
using ChatApp.Application.Dtos.Messages;
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
            CreateMap<PrivateMessage, PrivateMessageDto>()
            // Thêm các điều kiện kiểm tra null
            .ForMember(dest => dest.SenderDisplayName, opt =>
                opt.MapFrom(src => src.Sender != null ? src.Sender.DisplayName : "Unknown"))
            .ForMember(dest => dest.SenderAvatarUrl, opt =>
                opt.MapFrom(src => src.Sender != null ? src.Sender.AvatarUrl : null));
        }
    }
}