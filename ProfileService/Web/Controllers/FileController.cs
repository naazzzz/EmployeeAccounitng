using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Core.Interfaces.Services;

namespace ProfileService.Web.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _appEnvironment;
    private readonly IFileService _fileService;

    public FilesController(IWebHostEnvironment appEnvironment, IFileService fileService)
    {
        _appEnvironment = appEnvironment;
        _fileService = fileService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddFile(IFormFile uploadedFile)
    {
        return Ok(await _fileService.LoadFileToServer(uploadedFile, _appEnvironment.WebRootPath));
    }
}