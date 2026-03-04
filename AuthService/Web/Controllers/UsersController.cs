using AuthService.Core.Entities;
using AuthService.Core.Events.Listeners;
using AuthService.Core.Interfaces.Services;
using AuthService.Web.Dto.User;
using AutoMapper;
using General.Auth;
using General.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Web.Dto.User;

namespace AuthService.Web.Controllers;

[ApiController]
[Route("/api/[controller]")]
public sealed class UsersController(IUserService userService, IMapper mapper, AuditListener auditListener) : ControllerBase
{
    private readonly AuditListener _auditListener = auditListener;
    
    [HttpGet]
    [Authorize]
    public async Task<ActionResult> GetCollection()
    {
        var users = await userService.GetAll();
        if (users.Count == 0) return NotFound();

        var responseDto = mapper.Map<List<UserResponseDto>>(users);

        return Ok(responseDto);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateUserDto userDto)
    {
        var user = mapper.Map<User>(userDto);
        var resultUser = await userService.Create(user);
        var responseDto = mapper.Map<UserResponseDto>(resultUser);

        return Created("", responseDto);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult> GetItem(string id)
    {
        var user = await userService.GetById(id);
        if (user == null) return NotFound();

        var responseDto = mapper.Map<UserResponseDto>(user);

        return Ok(responseDto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(string id)
    {
        await userService.SoftDelete(id);
        return NoContent();
    }

    [HttpPatch("{id}/password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto, string id)
    {
        try
        {
            await userService.ChangePassword(id, changePasswordDto.NewPassword, changePasswordDto.OldPassword);
            return Ok(new
            {
                Message = "Код для подтверждения смены пароля, отправлен на текущую почту"
            });
        }
        catch (Exception e)
        {
            if (e is RecordNotFoundException)
                return NotFound(new
                {
                    Error = e.Message
                });

            throw;
        }
    }

    [HttpPatch("{id}/email")]
    //todo сделалть политику на самомго себя или админа
    [Authorize]
    public async Task<ActionResult> ChangeEmail(ChangeEmailDto changeEmailDto, string id)
    {
        try
        {
            await userService.ChangeEmail(id, changeEmailDto.Email);
            return Ok(new
            {
                Message = "Код для подтверждения смены почты, отправлен на текущую почту"
            });
        }
        catch (Exception e)
        {
            if (e is RecordNotFoundException)
                return NotFound(new
                {
                    Error = e.Message
                });

            throw;
        }
    }

    [HttpPatch("{id}/block")]
    [Authorize(Roles = Roles.RoleAdmin)]
    public async Task<ActionResult> BlockUser(string id)
    {
        try
        {
            await userService.BlockUser(id);
            return Ok(new
            {
                Message = $"Пользователь {id} успешно заблокирован"
            });
        }
        catch (Exception e)
        {
            if (e is RecordNotFoundException)
                return NotFound(new
                {
                    Error = e.Message
                });

            throw;
        }
    }

    [HttpPatch("{id}/unblock")]
    [Authorize(Roles = Roles.RoleAdmin)]
    public async Task<ActionResult> UnblockUser(string id)
    {
        try
        {
            await userService.UnblockUser(id);
            return Ok(new
            {
                Message = $"Пользователь {id} успешно разблокирован"
            });
        }
        catch (Exception e)
        {
            if (e is RecordNotFoundException)
                return NotFound(new
                {
                    Error = e.Message
                });

            throw;
        }
    }
    

    // Пример использования access_token в контексте работы приложения
    // [Authorize] // Требует аутентификации
    // public async Task<IActionResult> GetContacts()
    // {
    //     // Получаем access_token из контекста аутентификации
    //     var accessToken = await HttpContext.GetTokenAsync("access_token");
    //     if (string.IsNullOrEmpty(accessToken))
    //     {
    //         return BadRequest("No access token available.");
    //     }
    //
    //     // Создаём HTTP-клиент
    //     var client = _httpClientFactory.CreateClient();
    //     client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    //
    //     // Вызов Google People API для контактов
    //     var response = await client.GetAsync("https://people.googleapis.com/v1/people/me/connections?personFields=names,emailAddresses");
    //
    //     if (response.IsSuccessStatusCode)
    //     {
    //         var content = await response.Content.ReadAsStringAsync();
    //         // Здесь обработайте JSON (например, десериализуйте в модель)
    //         ViewBag.Contacts = content; // Для примера, передаём в View
    //         return View();
    //     }
    //     else
    //     {
    //         return BadRequest("Error fetching contacts.");
    //     }
    // }
}