using Application.Alarms.Dtos;
using Application.Triggers;
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
        AlarmSendingResult ShouldSendAlarmUnlessQuenched(BreachedTriggerDto alarm);
        Task<List<BreachedTriggerDto>> GetNotReportedBreachesAsync();
        Task SendAlarm(BreachedTriggerDto alarm);
        Task<RecipientSet> GetRecipientSetByAlarmIdAsync(int alarmId);
        Task<List<Alarm>> GetAlarmsByRecipientSetIdAsync(int recipientSetId);
    }
}