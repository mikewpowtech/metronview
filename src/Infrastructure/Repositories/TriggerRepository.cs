using Application.Triggers;
using Domain;
using Domain.Enums;
using Infrastructure.DbClasses;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TriggerRepository : ITriggerRepository
    {
        private readonly ApplicationDbContext _context;

        public TriggerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Trigger?> GetByIdAsync(int id)
        {
            var db = await _context.Triggers
                //.Include(t => t.Alarm)
                .Include(t => t.TriggerType)
                .Include(t => t.CommunicationMode)
                .FirstOrDefaultAsync(t => t.Id == id);

            return db == null ? null : db.Adapt<Trigger>();
        }

        public async Task<List<Trigger>> GetAllAsync()
        {
            var dbList = await _context.Triggers
                //.Include(t => t.Alarm)
                .Include(t => t.TriggerType)
                .Include(t => t.CommunicationMode)
                .OrderBy(t => t.Id)
                .ToListAsync();

            return dbList.Adapt<List<Trigger>>();
        }

        public async Task<List<Trigger>> GetByAlarmIdAsync(int alarmId)
        {
            var dbList = await _context.Triggers
                .Include(t => t.Alarm)
                .Include(t => t.TriggerType)
                .Include(t => t.CommunicationMode)
                .Where(t => t.AlarmId == alarmId)
                .OrderBy(t => t.Id)
                .ToListAsync();

            return dbList.Adapt<List<Trigger>>();
        }

        public async Task<List<Trigger>> GetByTriggerTypeCodeAsync(TriggerTypeCode triggerTypeCode)
        {
            var dbList = await _context.Triggers
                .Include(t => t.Alarm)
                .Include(t => t.TriggerType)
                .Include(t => t.CommunicationMode)
                .Where(t => t.TriggerTypeCode == triggerTypeCode)
                .OrderBy(t => t.Id)
                .ToListAsync();

            return dbList.Adapt<List<Trigger>>();
        }

        public async Task<List<Trigger>> GetByCommunicationModeIdAsync(int communicationModeId)
        {
            var dbList = await _context.Triggers
                .Include(t => t.Alarm)
                .Include(t => t.TriggerType)
                .Include(t => t.CommunicationMode)
                .Where(t => t.CommunicationModeId == communicationModeId)
                .OrderBy(t => t.Id)
                .ToListAsync();

            return dbList.Adapt<List<Trigger>>();
        }

        public async Task<List<Trigger>> GetEnabledTriggersAsync()
        {
            var dbList = await _context.Triggers
                .Include(t => t.Alarm)
                .Include(t => t.TriggerType)
                .Include(t => t.CommunicationMode)
                .Where(t => t.IsEnabled)
                .OrderBy(t => t.Id)
                .ToListAsync();

            return dbList.Adapt<List<Trigger>>();
        }

        public async Task<Trigger> AddAsync(Trigger trigger)
        {
            var db = trigger.Adapt<TriggerDb>();
            _context.Triggers.Add(db);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var reloaded = await GetByIdAsync(db.Id);
            return reloaded!;
        }

        public async Task<bool> UpdateAsync(Trigger trigger)
        {
            var db = await _context.Triggers.FirstOrDefaultAsync(t => t.Id == trigger.Id);
            if (db == null) return false;

            db.AlarmId = trigger.AlarmId;
            db.TriggerTypeCode = trigger.TriggerTypeCode;
            db.TriggerValue = trigger.TriggerValue;
            db.CommunicationModeId = trigger.CommunicationModeId;
            db.Subject = trigger.Subject;
            db.Body = trigger.Body;
            db.MinimumSendIntervalMinutes = trigger.MinimumSendIntervalMinutes;
            db.IsEnabled = trigger.IsEnabled;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var db = await _context.Triggers.FirstOrDefaultAsync(t => t.Id == id);
            if (db == null) return false;

            _context.Triggers.Remove(db);
            await _context.SaveChangesAsync();
            return true;
        }

        // sTriggerTypeId: the TriggerTypeId for 'S'
        public async Task<List<BreachedTriggerDto>> GetNotReportedBreachesAsync(TriggerTypeCode triggerTypeCode)
        {
            var now = DateTime.UtcNow;

            var query =
                from sensor in _context.Sensors
                join alarm in _context.Alarms on sensor.AlarmId equals alarm.Id
                join trigger in _context.Triggers on alarm.Id equals trigger.AlarmId
                join mostRecent in _context.MostRecentReadings on sensor.Id equals mostRecent.SensorId into readingJoin
                from mostRecent in readingJoin.DefaultIfEmpty()
                where trigger.IsEnabled
                    && trigger.TriggerTypeCode == triggerTypeCode
                    && (
                        mostRecent == null ||
                        mostRecent.DateRecordedUtc == null ||
                        now.AddMinutes(trigger.TriggerValue * -1) > mostRecent.DateRecordedUtc
                    )
                select new BreachedTriggerDto
                {
                    SensorId = sensor.Id,
                    TriggerTypeCode = trigger.TriggerTypeCode,
                    TriggerValue = trigger.TriggerValue,
                    CommunicationModeId = trigger.CommunicationModeId,
                    Subject = trigger.Subject,
                    Body = trigger.Body,
                    Value = mostRecent.Value,
                    IsAlarm = null, // MostRecentReadings.IsAlarm not present in schema, set as null or add if available
                    DateRecordedUtc = mostRecent.DateRecordedUtc,
                    PendingAlarmTriggerId = null, // No equivalent in EF, set as null
                    AlarmSetId = alarm.Id, // Alarm is AlarmSet in this mapping
                    TriggerId = trigger.Id,
                    MinimumSendIntervalMinutes = trigger.MinimumSendIntervalMinutes
                };

            return await query.ToListAsync();
        }
    }
}
