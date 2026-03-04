using General.Interfaces;
using File = ProfileService.Core.Domain.Entities.File;

namespace ProfileService.Core.Interfaces.Repositories;

public interface IFileRepository : IRepository<File>
{
}