using General.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Core.Interfaces.Services;
using ProfileService.Web.Dto.Avatar;

namespace ProfileService.Web.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
//todo добавить фильтрацию по своим аватарам
public class AvatarsController : ControllerBase
{
    private readonly IAvatarService _avatarService;

    public AvatarsController(IAvatarService avatarService)
    {
        _avatarService = avatarService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAvatarDto createAvatarDto)
    {
        var avatar = await _avatarService.Create(createAvatarDto.ProfileId, createAvatarDto.FileId);
        if (avatar == null) return NotFound();

        return new CreatedResult("", avatar);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateFile(UpdateAvatarDto updateAvatarDto, string id)
    {
        try
        {
            var avatar = await _avatarService.ChangeFile(id, updateAvatarDto.FileId);
            return Ok(avatar);
        }
        catch (Exception e)
        {
            if (e is RecordNotFoundException) return NotFound();

            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCollection()
    {
        try
        {
            var avatar = await _avatarService.GetCollection();
            return Ok(avatar);
        }
        catch (Exception e)
        {
            if (e is RecordNotFoundException) return NotFound();

            throw;
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetItem(string id)
    {
        try
        {
            var avatar = await _avatarService.GetItem(id);
            return Ok(avatar);
        }
        catch (Exception e)
        {
            if (e is RecordNotFoundException) return NotFound();

            throw;
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _avatarService.Delete(id);
            return NoContent();
        }
        catch (Exception e)
        {
            if (e is RecordNotFoundException) return NotFound();

            throw;
        }
    }
}