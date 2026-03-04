using ProfileService.Core.Domain.Entities;
using ProfileService.Core.Interfaces.Repositories;
using ProfileService.Core.Interfaces.Services;

namespace ProfileService.Core.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<Department?> FindDefaultDepartment()
    {
        var defaultDepartment = await _departmentRepository.FindDefaultDepartment();
        if (defaultDepartment == null)
        {
            defaultDepartment = new Department
            {
                Name = "Default"
            };

            defaultDepartment = await _departmentRepository.AddWithSaveAsync(defaultDepartment);
        }

        return defaultDepartment;
    }
}