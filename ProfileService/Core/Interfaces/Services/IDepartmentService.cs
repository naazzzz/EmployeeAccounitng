using ProfileService.Core.Domain.Entities;

namespace ProfileService.Core.Interfaces.Services;

public interface IDepartmentService
{
    Task<Department?> FindDefaultDepartment();
}