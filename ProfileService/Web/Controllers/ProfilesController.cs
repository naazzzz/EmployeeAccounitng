using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Core.Interfaces.Services;
using ProfileService.Web.Dto.User;
using Profile = ProfileService.Core.Domain.Entities.Profile;

namespace ProfileService.Web.Controllers;

[ApiController]
[Route("/api/[controller]")]
public sealed class ProfilesController(IProfileService profileService, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult> GetCollection()
    {
        var profiles = await profileService.GetAll();
        if (profiles.Count == 0) return NotFound();

        var responseDto = mapper.Map<List<ProfileResponseDto>>(profiles);

        return Ok(responseDto);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create(CreateProfileDto profileDto)
    {
        var user = mapper.Map<Profile>(profileDto);
        var resultUser = await profileService.Create(user);
        var responseDto = mapper.Map<ProfileResponseDto>(resultUser);

        return Created("", responseDto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult> GetItem(string id)
    {
        var profile = await profileService.GetById(id);
        if (profile == null) return NotFound();

        var responseDto = mapper.Map<ProfileResponseDto>(profile);

        return Ok(responseDto);
    }

    // [HttpGet("me")]
    // public async Task<ActionResult> GetItem()
    // {
    //     var user = await userService.GetById(id);
    //     if (user == null)
    //     {
    //         return NotFound();
    //     }
    //
    //     return Ok(user);
    // }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateProfileInfo(UpdateProfileUserDto updateUserDto, string id)
    { 
        var user = mapper.Map<Profile>(updateUserDto);
        user = await profileService.UpdateProfileInfo(id, user);
        var responseDto = mapper.Map<ProfileResponseDto>(user);

        return Ok(responseDto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(string id)
    {
        await profileService.SoftDelete(id);
        return NoContent();
    }
}