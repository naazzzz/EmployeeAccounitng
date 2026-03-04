using ProfileService.Core.Interfaces.Repositories;
using ProfileService.Core.Interfaces.Services;
using File = ProfileService.Core.Domain.Entities.File;

namespace ProfileService.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;

    public FileService(IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    public async Task<File?> LoadFileToServer(IFormFile uploadedFile, string rootPath)
    {
        var path = "/Files/" + uploadedFile.FileName;
        using (var fileStream = new FileStream(rootPath + path, FileMode.Create))
        {
            await uploadedFile.CopyToAsync(fileStream);
        }

        var file = new File { Name = uploadedFile.FileName, Path = path };

        await _fileRepository.AddWithSaveAsync(file);

        return file;
    }
}