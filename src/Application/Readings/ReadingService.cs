using Application.Triggers;
using Domain;

namespace Application.Readings;

public class ReadingService : IReadingService
{
    private readonly IReadingRepository _readingRepository;

    public ReadingService(IReadingRepository readingRepository)
    {
        _readingRepository = readingRepository;
    }

    public Task<Reading?> GetByIdAsync(DateTime dateRecordedUtc, int sensorId)
        => _readingRepository.GetByIdAsync(dateRecordedUtc, sensorId);

    public Task<List<Reading>> GetAllAsync()
        => _readingRepository.GetAllAsync();

    public Task<List<Reading>> GetBySensorIdAsync(int sensorId)
        => _readingRepository.GetBySensorIdAsync(sensorId);

    public Task<List<Reading>> GetByUnitIdAsync(int unitId)
        => _readingRepository.GetByUnitIdAsync(unitId);

    public Task<Reading> AddAsync(Reading reading)
        => _readingRepository.AddAsync(reading);

    public Task<bool> UpdateAsync(Reading reading)
        => _readingRepository.UpdateAsync(reading);

    public Task<bool> DeleteAsync(DateTime dateRecordedUtc, int sensorId)
        => _readingRepository.DeleteAsync(dateRecordedUtc, sensorId);

    public IList<BreachedTriggerDto> GetNewReadingsWithAlarms()
    {
        throw new NotImplementedException();
    }
}