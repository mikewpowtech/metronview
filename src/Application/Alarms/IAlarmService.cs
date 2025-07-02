using Domain;

namespace Application.Alarms
{
    public interface IAlarmService
    {
        Task<Alarm?> GetByIdAsync(int id);
        Task<List<Alarm>> GetAllAsync();
        Task<Alarm> AddAsync(Alarm alarm);
        Task<bool> UpdateAsync(Alarm alarm);
        Task<bool> DeleteAsync(int id);
    }
}