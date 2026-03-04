using General.Base;
using Microsoft.EntityFrameworkCore;
using ProfileService.Core.Domain.Entities;
using ProfileService.Core.Interfaces.Repositories;
using ApplicationContext = ProfileService.Infrastructure.DbContext.ApplicationContext;

namespace ProfileService.Infrastructure.Repositories;

public class DepartmentRepository(ApplicationContext context)
    : BaseRepository<Department>(context), IDepartmentRepository
{
    private readonly ApplicationContext _context = context;
    
    public async Task<Department?> FindDefaultDepartment()
    {
        return await _context.Departments.FirstOrDefaultAsync();
    }
}