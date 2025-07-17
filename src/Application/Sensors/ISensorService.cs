using Application.Triggers;
using Domain;

namespace Application.Sensors;

public interface ISensorService
{
    Task<Sensor?> GetByIdAsync(int id);
    Task<List<Sensor>> GetByUnitIdAsync(int unitId);
    Task<List<Sensor>> GetByCompanyIdAsync(int companyId);
    Task<List<Sensor>> GetByAlarmIdAsync(int alarmId);
    Task<List<Sensor>> GetAllAsync();
    Task<Sensor> AddAsync(Sensor sensor);
    Task<bool> UpdateAsync(Sensor sensor);
    Task<bool> DeleteAsync(int id);
    Task CalculateSensorAsync(BreachedTriggerDto triggerDto);
}