using AuthService.Core.Interfaces;
using AuthService.Web.Dto.MailCode;
using General.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService.Web.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize(policy: "DefaultWithNotConfirmedMail")]
public sealed class CodesController: ControllerBase
{
    private IСonfirmationCodeService _codeService;

    public CodesController(IСonfirmationCodeService codeService)
    {
        _codeService = codeService;
    }
    
    [EnableRateLimiting("FixedConfirmCodePolicy")]
    [HttpPatch]
    [Route("confirm")]
    public async Task<ActionResult> Confirm2FaChanges(ConfirmMailDto confirmMailDto)
    {
        try
        {
            await _codeService.FindAndAcceptCode(confirmMailDto.Code, confirmMailDto.UserId);
            
            return Ok(new
            {
                Message = "Код для двухфакторной авторизации успешно принят",
            });
        }
        catch (RecordNotFoundException e)
        {
            
                return NotFound(new JsonResult(new
                {
                    Error = e.Message,
                }));
        }
    }
    
    [EnableRateLimiting("CreateCodePolicy")]
    [HttpPost]
    public async Task<ActionResult> CreateNewCode(CreateCodeDto createCodeDto)
    {
        try
        {
           await _codeService.CreateNewCodeForUserByRefreshCode(createCodeDto.UserId, createCodeDto.RefreshCode!);
        }
        catch (Exception e)
        {
            if (e is RecordNotFoundException)
            {
                return NotFound();
            }
            
            Console.WriteLine(e);
            throw;
        }
        
        return Ok(new
        {
            Message = "Код подтверждения отправлен на почту",
        });
    }
}