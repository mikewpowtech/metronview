using Domain;
using Application.ConfigurationUploads;
using Microsoft.EntityFrameworkCore;
using Mapster;
using Infrastructure.DbClasses;

namespace Infrastructure.Repositories;

public class ConfigurationUploadRepository : IConfigurationUploadRepository
{
    private readonly ApplicationDbContext _context;

    public ConfigurationUploadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ConfigurationUpload>> GetAllAsync()
    {
        return await _context.ConfigurationUploads
            .ProjectToType<ConfigurationUpload>()
            .ToListAsync();
    }

    public async Task<ConfigurationUpload?> GetByIdAsync(int id)
    {
        return await _context.ConfigurationUploads
            .Where(x => x.Id == id)
            .ProjectToType<ConfigurationUpload>()
            .FirstOrDefaultAsync();
    }

    public async Task<List<ConfigurationUpload>> GetByUnitIdAsync(int unitId)
    {
        return await _context.ConfigurationUploads
            .Where(cu => cu.UnitId == unitId)
            .ProjectToType<ConfigurationUpload>()
            .ToListAsync();
    }

    public async Task AddAsync(ConfigurationUpload upload)
    {
        // Mapster is not needed for adding, as you should add the Db entity
        var dbEntity = upload.Adapt<ConfigurationUploadDb>();
        _context.ConfigurationUploads.Add(dbEntity);
        await _context.SaveChangesAsync();
    }
}