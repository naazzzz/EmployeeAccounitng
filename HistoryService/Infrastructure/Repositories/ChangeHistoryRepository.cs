using General.Base;
using HistoryService.Core.Domain.Entity;
using HistoryService.Core.Interfaces;

namespace HistoryService.Infrastructure.Repositories;

public class ChangeHistoryRepository(ApplicationContext context)
    : BaseRepository<ChangeHistory>(context), IChangeHistoryRepository
{
    private readonly ApplicationContext _context = context;
}