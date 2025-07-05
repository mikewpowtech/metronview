using Domain;

namespace Application.Readings;

public interface IReadingService
{
    Task<Reading?> GetByIdAsync(DateTime dateRecordedUtc, int sensorId);
    Task<List<Reading>> GetAllAsync();
    Task<List<Reading>> GetBySensorIdAsync(int sensorId);
    Task<List<Reading>> GetByUnitIdAsync(int unitId);
    Task<Reading> AddAsync(Reading reading);
    Task<bool> UpdateAsync(Reading reading);
    Task<bool> DeleteAsync(DateTime dateRecordedUtc, int sensorId);
}