using File = ProfileService.Core.Domain.Entities.File;

namespace ProfileService.Core.Interfaces.Services;

public interface IFileService
{
    Task<File?> LoadFileToServer(IFormFile uploadedFile, string rootPath);
}