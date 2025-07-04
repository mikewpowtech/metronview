using Domain;

namespace Application.Sensors;

public class SensorService : ISensorService
{
    private readonly ISensorRepository _sensorRepository;

    public SensorService(ISensorRepository sensorRepository)
    {
        _sensorRepository = sensorRepository;
    }

    public Task<Sensor?> GetByIdAsync(int id) => _sensorRepository.GetByIdAsync(id);

    public Task<List<Sensor>> GetByUnitIdAsync(int unitId) => _sensorRepository.GetByUnitIdAsync(unitId);

    public Task<List<Sensor>> GetByCompanyIdAsync(int companyId) => _sensorRepository.GetByCompanyIdAsync(companyId);

    public Task<List<Sensor>> GetByAlarmIdAsync(int alarmId) => _sensorRepository.GetByAlarmIdAsync(alarmId);

    public Task<List<Sensor>> GetAllAsync() => _sensorRepository.GetAllAsync();

    public Task<Sensor> AddAsync(Sensor sensor) => _sensorRepository.AddAsync(sensor);

    public Task<bool> UpdateAsync(Sensor sensor) => _sensorRepository.UpdateAsync(sensor);

    public Task<bool> DeleteAsync(int id) => _sensorRepository.DeleteAsync(id);
}