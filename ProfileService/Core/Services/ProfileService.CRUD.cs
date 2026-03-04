using AutoMapper;
using General.Event.NotificationService;
using General.Exceptions;
using MassTransit;
using ProfileService.Core.Interfaces.Repositories;
using ProfileService.Core.Interfaces.Services;
using Profile = ProfileService.Core.Domain.Entities.Profile;

namespace ProfileService.Core.Services;

public sealed class ProfileService : IProfileService
{
    private readonly IDepartmentService _departmentService;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IProfileRepository _profileRepository;

    public ProfileService(IProfileRepository profileRepository, IMapper mapper,
        IDepartmentService departmentService, IPublishEndpoint publishEndpoint)
    {
        _profileRepository = profileRepository;
        _mapper = mapper;
        _departmentService = departmentService;
        _publishEndpoint = publishEndpoint;
    }

    public Task<List<Profile>> GetAll()
    {
        return _profileRepository.GetAllAsync();
    }

    public Task<Profile?> GetById(string id)
    {
        return _profileRepository.GetByIdAsync(id);
    }

    public async Task<Profile?> Create(Profile profile)
    {
        if (profile.DepartmentId == null)
        {
            var department = await _departmentService.FindDefaultDepartment();

            profile.DepartmentId = department!.Id;
            profile.Department = department;
        }

        var taskProfile = await _profileRepository.AddWithSaveAsync(profile);
        return taskProfile;
    }

    public async Task<Profile?> UpdateProfileInfo(string id, Profile updateProfile)
    {
        updateProfile.Id = id;

        var task = GetById(id);
        if (task.Result == null) throw new RecordNotFoundException(id, "User");

        var user = task.Result;

        _mapper.Map(updateProfile, user);

        if (updateProfile.DepartmentId != null)
        {
            // todo по-хорошему в интерсептор вынести
            await _publishEndpoint.Publish(new UserTransferredEvent
            {
                DepartmentName = user.Department!.Name,
                UserName = user.UserName!,
                Email = user.Email!
            });
        }
        
        await _profileRepository.SaveChanges();

        return user;
    }

    public Task SoftDelete(string id)
    {
        return _profileRepository.DeleteWithSaveAsync(id);
    }
}