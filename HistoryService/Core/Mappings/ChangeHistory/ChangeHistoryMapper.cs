using AutoMapper;
using General.Dto;

namespace HistoryService.Core.Mappings.ChangeHistory;

public sealed class ChangeHistoryMapper : Profile
{
    public ChangeHistoryMapper()
    {
        CreateMap<ChangeHistoryDto, HistoryService.Core.Domain.Entity.ChangeHistory>();
        CreateMap<HistoryService.Core.Domain.Entity.ChangeHistory, ChangeHistoryDto>();
    }
}