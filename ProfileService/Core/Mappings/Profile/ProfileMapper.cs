using ProfileService.Web.Dto.User;

namespace ProfileService.Core.Mappings.Profile;

public sealed class ProfileMapper : AutoMapper.Profile
{
    public ProfileMapper()
    {
        CreateMap<CreateProfileDto, Domain.Entities.Profile>();
        CreateMap<Domain.Entities.Profile, ProfileResponseDto>();
        CreateMap<Domain.Entities.Profile, Domain.Entities.Profile>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<UpdateProfileUserDto, Domain.Entities.Profile>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}