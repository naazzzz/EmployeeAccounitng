using AutoMapper;
using General.Dto;

namespace HistoryService.Core.Mappings.ChangeHistory;

public sealed class UserChangeHistoryMapper : Profile
{
    public UserChangeHistoryMapper()
    {
        CreateMap<UserChangeHistoryDto, HistoryService.Core.Domain.Entity.UserChangeHistory>();
        CreateMap<HistoryService.Core.Domain.Entity.UserChangeHistory, UserChangeHistoryDto>();
    }
}