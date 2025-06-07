using Domain;

namespace Application.Sensors;

public class SensorService : ISensorService
{
    private readonly ISensorRepository _sensorRepository;

    public SensorService(ISensorRepository sensorRepository)
    {
        _sensorRepository = sensorRepository;
    }

    public Task<Sensor?> GetByIdAsync(string id) => _sensorRepository.GetByIdAsync(id);

    public Task<List<Sensor>> GetAllAsync() => _sensorRepository.GetAllAsync();

    public Task<Sensor> AddAsync(Sensor sensor) => _sensorRepository.AddAsync(sensor);

    public Task<bool> UpdateAsync(Sensor sensor) => _sensorRepository.UpdateAsync(sensor);

    public Task<bool> DeleteAsync(string id) => _sensorRepository.DeleteAsync(id);
}