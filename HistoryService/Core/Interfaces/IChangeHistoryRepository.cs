using General.Interfaces;
using HistoryService.Core.Domain.Entity;

namespace HistoryService.Core.Interfaces;

public interface IChangeHistoryRepository: IRepository<ChangeHistory>
{
}