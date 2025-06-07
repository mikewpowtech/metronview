using Application.Units;
using Domain;
using Infrastructure.DbClasses;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UnitRepository : IUnitRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UnitRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public UnitRepository() { }

    public async Task<Unit?> GetByIdAsync(string id)
    {
        var entity = await _context.Units
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
        return entity == null ? null : _mapper.Map<Unit>(entity);
    }

    public async Task<List<Unit>> GetAllAsync()
    {
        var entities = await _context.Units
            .AsNoTracking()
            .ToListAsync();
        return _mapper.Map<List<Unit>>(entities);
    }

    public async Task<Unit> AddAsync(Unit unit)
    {
        var entity = _mapper.Map<UnitDb>(unit);
        if (string.IsNullOrEmpty(entity.Id))
            entity.Id = Guid.NewGuid().ToString();
        _context.Units.Add(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<Unit>(entity);
    }

    public async Task<bool> UpdateAsync(Unit unit)
    {
        var existing = await _context.Units.FindAsync(unit.Id);
        if (existing == null) return false;

        _mapper.Map(unit, existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var entity = await _context.Units.FindAsync(id);
        if (entity == null) return false;

        _context.Units.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}