using General.Base;
using ProfileService.Core.Interfaces.Repositories;
using ApplicationContext = ProfileService.Infrastructure.DbContext.ApplicationContext;
using File = ProfileService.Core.Domain.Entities.File;

namespace ProfileService.Infrastructure.Repositories;

public class FileRepository(ApplicationContext context) : BaseRepository<File>(context), IFileRepository;