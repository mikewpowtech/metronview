using Domain;

namespace Application.Alarms
{
    public interface IAlarmRepository
    {
        Task<Alarm?> GetByIdAsync(int id);
        Task<List<Alarm>> GetAllAsync();
        Task<Alarm> AddAsync(Alarm alarm);
        Task<bool> UpdateAsync(Alarm alarm);
        Task<bool> DeleteAsync(int id);
    }
}