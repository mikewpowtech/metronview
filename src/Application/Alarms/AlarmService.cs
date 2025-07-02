using Domain;

namespace Application.Alarms
{
    public class AlarmService : IAlarmService
    {
        private readonly IAlarmRepository _repository;

        public AlarmService(IAlarmRepository repository)
        {
            _repository = repository;
        }

        public Task<Alarm?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<List<Alarm>> GetAllAsync() => _repository.GetAllAsync();
        public Task<Alarm> AddAsync(Alarm alarm) => _repository.AddAsync(alarm);
        public Task<bool> UpdateAsync(Alarm alarm) => _repository.UpdateAsync(alarm);
        public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}