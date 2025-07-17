using Application.Triggers;
using Application.Units;
using Domain;
using Infrastructure.DbClasses;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UnitRepository : IUnitRepository
{
    private readonly ApplicationDbContext context;
    private readonly IMapper _mapper;

    public UnitRepository(ApplicationDbContext context, IMapper mapper)
    {
        this.context = context;
        _mapper = mapper;
    }

    public UnitRepository() { }

    public async Task<Unit?> GetByIdAsync(int id)
    {
        var entity = await context.Units
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
        return entity == null ? null : _mapper.Map<Unit>(entity);
    }

    public async Task<List<Unit>> GetAllAsync()
    {
        var entities = await context.Units
            .AsNoTracking()
            .ToListAsync();
        return _mapper.Map<List<Unit>>(entities);
    }

    public async Task<Unit> AddAsync(Unit unit)
    {
        var entity = _mapper.Map<UnitDb>(unit);

        context.Units.Add(entity);
        await context.SaveChangesAsync();
        return _mapper.Map<Unit>(entity);
    }

    public async Task<bool> UpdateAsync(Unit unit)
    {
        var existing = await context.Units.FindAsync(unit.Id);
        if (existing == null) return false;

        _mapper.Map(unit, existing);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await context.Units.FindAsync(id);
        if (entity == null) return false;

        context.Units.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }

    //TODO refactor ewwwwww hate this overloading parameters
    public async Task CalculateRtuAsync(BreachedTriggerDto alarmTriggerTemplateValues)
    {
        var unitInfo = await context.Sensors
            .AsNoTracking()
            .Where(s => s.Id == alarmTriggerTemplateValues.SensorId)
            .Join(context.Units,
                sensor => sensor.UnitId,
                unit => unit.Id,
                (sensor, unit) => new
                {
                    RtuId = unit.Id,
                    PhoneNumber = unit.PhoneNumber,
                    ManufacturerId = unit.ManufacturerCode,
                    ClientId = unit.UnitCode
                })
            .FirstOrDefaultAsync();

        if (unitInfo != null)
        {
            alarmTriggerTemplateValues.SetKnownValue("rtuid", unitInfo.RtuId);
            alarmTriggerTemplateValues.SetKnownValue("phonenumber", unitInfo.PhoneNumber);
            alarmTriggerTemplateValues.SetKnownValue("manufacturerid", unitInfo.ManufacturerId);
            alarmTriggerTemplateValues.SetKnownValue("clientid", unitInfo.ClientId);
        }
    }
}