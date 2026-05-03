using ConfigService.Domain.Entities;
using ConfigService.Infrastructure.Data;
using ConfigService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConfigService.Infrastructure.Repositories;

public class ConfigurationRepository : IConfigurationRepository
{
    private readonly ConfigDbContext _context;
    
    public ConfigurationRepository(ConfigDbContext context)
    {
        _context = context;
    }
    
    public async Task<Configuration?> GetByIdAsync(string id)
    {
        return await _context.Configurations
            .Include(c => c.Application)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<Configuration?> GetByApplicationAndNameAsync(string applicationId, string name)
    {
        return await _context.Configurations
            .FirstOrDefaultAsync(c => c.ApplicationId == applicationId && c.Name == name);
    }
    
    public async Task<IEnumerable<Configuration>> GetByApplicationIdAsync(string applicationId)
    {
        return await _context.Configurations
            .Where(c => c.ApplicationId == applicationId)
            .ToListAsync();
    }
    
    public async Task<Configuration> CreateAsync(Configuration configuration)
    {
        _context.Configurations.Add(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }
    
    public async Task<Configuration> UpdateAsync(Configuration configuration)
    {
        _context.Configurations.Update(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }
    
    public async Task DeleteAsync(string id)
    {
        var configuration = await GetByIdAsync(id);
        if (configuration != null)
        {
            _context.Configurations.Remove(configuration);
            await _context.SaveChangesAsync();
        }
    }
}
