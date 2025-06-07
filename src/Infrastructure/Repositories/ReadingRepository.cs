using Application.Readings;
using Domain;
using Infrastructure.DbClasses;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReadingRepository : IReadingRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ReadingRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Reading?> GetByIdAsync(DateTime dateRecordedUtc, int sensorId)
    {
        var entity = await _context.Readings
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.DateRecordedUtc == dateRecordedUtc && r.SensorId == sensorId);
        return entity == null ? null : _mapper.Map<Reading>(entity);
    }

    public async Task<List<Reading>> GetAllAsync()
    {
        var entities = await _context.Readings
            .AsNoTracking()
            .ToListAsync();
        return _mapper.Map<List<Reading>>(entities);
    }

    public async Task<Reading> AddAsync(Reading reading)
    {
        var entity = _mapper.Map<ReadingDb>(reading);
        _context.Readings.Add(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<Reading>(entity);
    }

    public async Task<bool> UpdateAsync(Reading reading)
    {
        var dbReading = _mapper.Map<ReadingDb>(reading);
        var existing = await _context.Readings.FindAsync(dbReading.DateRecordedUtc, dbReading.SensorId);
        if (existing == null) return false;

        _mapper.Map(reading, existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(DateTime dateRecordedUtc, int sensorId)
    {
        var entity = await _context.Readings.FindAsync(dateRecordedUtc, sensorId);
        if (entity == null) return false;

        _context.Readings.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}