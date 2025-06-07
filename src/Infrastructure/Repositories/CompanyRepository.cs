using Application.Companies;
using Domain;
using Infrastructure.DbClasses;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CompanyRepository(ApplicationDbContext context, IMapper mapper) : ICompanyRepository
{
    public async Task<Company?> GetByIdAsync(string id)
    {
        var entity = await context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        return entity == null ? null : mapper.Map<Company>(entity);
    }

    public async Task<List<Company>> GetAllAsync()
    {
        var entities = await context.Companies
            .AsNoTracking()
            .ToListAsync();
        return mapper.Map<List<Company>>(entities);
    }

    public async Task<Company> AddAsync(Company company)
    {
        var entity = mapper.Map<CompanyDb>(company);
        if (string.IsNullOrEmpty(entity.Id))
            entity.Id = Guid.NewGuid().ToString();
        context.Companies.Add(entity);
        await context.SaveChangesAsync();
        return mapper.Map<Company>(entity);
    }

    public async Task<bool> UpdateAsync(Company company)
    {
        var existing = await context.Companies.FindAsync(company.Id);
        if (existing == null) return false;

        mapper.Map(company, existing);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var company = await context.Companies.FindAsync(id);
        if (company == null) return false;

        context.Companies.Remove(company);
        await context.SaveChangesAsync();
        return true;
    }
}