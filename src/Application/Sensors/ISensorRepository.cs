using Domain;

namespace Application.Sensors;

public interface ISensorRepository
{
    Task<Sensor?> GetByIdAsync(int id);
    Task<List<Sensor>> GetByUnitIdAsync(int unitId);
    Task<List<Sensor>> GetByCompanyIdAsync(int companyId);
    Task<List<Sensor>> GetByAlarmIdAsync(int alarmId);
    Task<List<Sensor>> GetAllAsync();
    Task<Sensor> AddAsync(Sensor sensor);
    Task<bool> UpdateAsync(Sensor sensor);
    Task<bool> DeleteAsync(int id);
}