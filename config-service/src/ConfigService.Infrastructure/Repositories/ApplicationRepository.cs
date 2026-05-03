using ConfigService.Domain.Entities;
using ConfigService.Infrastructure.Data;
using ConfigService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using AppEntity = ConfigService.Domain.Entities.Application;

namespace ConfigService.Infrastructure.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly ConfigDbContext _context;
    
    public ApplicationRepository(ConfigDbContext context)
    {
        _context = context;
    }
    
    public async Task<AppEntity?> GetByIdAsync(string id)
    {
        return await _context.Applications
            .Include(a => a.Configurations)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task<AppEntity?> GetByNameAsync(string name)
    {
        return await _context.Applications.FirstOrDefaultAsync(a => a.Name == name);
    }
    
    public async Task<IEnumerable<AppEntity>> GetAllAsync()
    {
        return await _context.Applications
            .Include(a => a.Configurations)
            .ToListAsync();
    }
    
    public async Task<AppEntity> CreateAsync(AppEntity application)
    {
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }
    
    public async Task<AppEntity> UpdateAsync(AppEntity application)
    {
        _context.Applications.Update(application);
        await _context.SaveChangesAsync();
        return application;
    }
    
    public async Task DeleteAsync(string id)
    {
        var application = await GetByIdAsync(id);
        if (application != null)
        {
            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
        }
    }
}
