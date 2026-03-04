using AuthService.Web.Dto.User;
using AutoMapper;

namespace AuthService.Core.Mappings.User;

public sealed class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<CreateUserDto, Entities.User>();
        CreateMap<Entities.User, UserResponseDto>();
        CreateMap<Entities.User, Entities.User>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}