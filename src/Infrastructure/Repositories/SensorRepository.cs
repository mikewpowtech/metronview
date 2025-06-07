using Application.Sensors;
using Domain;
using Infrastructure.DbClasses;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SensorRepository : ISensorRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SensorRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Sensor?> GetByIdAsync(int id)
    {
        var entity = await _context.Sensors
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
        return entity == null ? null : _mapper.Map<Sensor>(entity);
    }

    public async Task<List<Sensor>> GetAllAsync()
    {
        var entities = await _context.Sensors
            .AsNoTracking()
            .ToListAsync();
        return _mapper.Map<List<Sensor>>(entities);
    }

    public async Task<Sensor> AddAsync(Sensor sensor)
    {
        var entity = _mapper.Map<SensorDb>(sensor);
        _context.Sensors.Add(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<Sensor>(entity);
    }

    public async Task<bool> UpdateAsync(Sensor sensor)
    {
        var existing = await _context.Sensors.FindAsync(sensor.Id);
        if (existing == null) return false;

        _mapper.Map(sensor, existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Sensors.FindAsync(id);
        if (entity == null) return false;

        _context.Sensors.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}