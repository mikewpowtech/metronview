using Application.Units;
using Domain;
using Infrastructure.DbClasses;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UnitModelRepository : IUnitModelRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UnitModelRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UnitModel?> GetByIdAsync(int id)
    {
        var entity = await _context.Set<UnitModelDb>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
        return entity == null ? null : _mapper.Map<UnitModel>(entity);
    }

    public async Task<List<UnitModel>> GetAllAsync()
    {
        var entities = await _context.Set<UnitModelDb>()
            .AsNoTracking()
            .ToListAsync();
        return _mapper.Map<List<UnitModel>>(entities);
    }

    public async Task<UnitModel> AddAsync(UnitModel unitModel)
    {
        var entity = _mapper.Map<UnitModelDb>(unitModel);
        _context.Set<UnitModelDb>().Add(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<UnitModel>(entity);
    }

    public async Task<bool> UpdateAsync(UnitModel unitModel)
    {
        var existing = await _context.Set<UnitModelDb>().FindAsync(unitModel.Id);
        if (existing == null) return false;

        _mapper.Map(unitModel, existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Set<UnitModelDb>().FindAsync(id);
        if (entity == null) return false;

        _context.Set<UnitModelDb>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}