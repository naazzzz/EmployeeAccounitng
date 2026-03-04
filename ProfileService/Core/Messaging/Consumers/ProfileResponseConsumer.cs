using AutoMapper;
using General.Event.ProfileService;
using MassTransit;
using ProfileService.Core.Interfaces.Services;
using Profile = ProfileService.Core.Domain.Entities.Profile;

namespace ProfileService.Core.Messaging.Consumers;

public class ProfileResponseConsumer : IConsumer<CreateDefaultUserProfileEvent>
{
    private readonly IProfileService _profileService;
    private readonly ILogger<ProfileResponseConsumer> _logger;
    private readonly IMapper _mapper;

    public ProfileResponseConsumer(IProfileService profileService, ILogger<ProfileResponseConsumer> logger, IMapper mapper)
    {
        _profileService = profileService;
        _logger = logger;
        _mapper = mapper;
    }

    // public async Task Consume(ConsumeContext<ProfileRequest> context)
    // {
    //     _logger.LogInformation("Received request for user {UserId}", context.Message.UserId);
    //         
    //     var profile = await _profileService.GetById(context.Message.UserId);
    //     if (profile == null)
    //     {
    //         _logger.LogWarning("User {UserId} not found", context.Message.UserId);
    //         await context.RespondAsync(new ProfileResponse(null));
    //         return;
    //     }
    //     
    //     var responseDto = _mapper.Map<ProfileResponseDto>(profile);
    //
    //     await context.RespondAsync(new ProfileResponse(responseDto));
    // }


    public async Task Consume(ConsumeContext<CreateDefaultUserProfileEvent> context)
    {
        _logger.LogInformation("Create Default profile for user {UserId}", context.Message.UserId);
        
        var profile = await _profileService.Create( new Profile
        {
            UserId = context.Message.UserId,
            UserName = context.Message.UserName,
            Email = context.Message.Email
        } );
        if (profile == null)
        {
            _logger.LogWarning("Failed to create a profile for the user with ID {UserId}", context.Message.UserId);
        }
    }
}