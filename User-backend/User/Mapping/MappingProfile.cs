using AutoMapper;
using User.DTOs;
using User.Entities;

namespace User.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // DTO -> Entity
            CreateMap<RegisterDto, EntitieUser>();

            // Entity -> DTO
            CreateMap<RegisterDto, EntitieUser>()
                .ForMember(dest => dest.PasswordHash,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Role,
                    opt => opt.Ignore());

            CreateMap<AddUserDto, RegisterDto>();
            CreateMap<EntitieUser, UserDisplayDto>();
        }
    }
}

