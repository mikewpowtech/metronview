using Application.Triggers;
using Domain;
using Domain.Enums;
using Infrastructure.DbClasses;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class TriggerRepository : ITriggerRepository
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<TriggerRepository> logger;

        public TriggerRepository(ApplicationDbContext context, ILogger<TriggerRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Trigger?> GetByIdAsync(int id)
        {
            var db = await context.Triggers
                //.Include(t => t.Alarm)
                .Include(t => t.TriggerType)
                .Include(t => t.CommunicationMode)
                .FirstOrDefaultAsync(t => t.Id == id);

            return db == null ? null : db.Adapt<Trigger>();
        }

        public async Task<List<Trigger>> GetAllAsync()
        {
            var dbList = await context.Triggers
                //.Include(t => t.Alarm)
                .Include(t => t.TriggerType)
                .Include(t => t.CommunicationMode)
                .OrderBy(t => t.Id)
                .ToListAsync();

             return dbList.Adapt<List<Trigger>>();
        }

        public async Task<List<Trigger>> GetByAlarmIdAsync(int alarmId)
        {
            var dbList = await context.Triggers
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
            var dbList = await context.Triggers
                .Include(t => t.Alarm)
                .Include(t => t.TriggerType)
                .Include(t => t.CommunicationMode)
                .Where(t => t.TriggerType.Code == triggerTypeCode)
                .OrderBy(t => t.Id)
                .ToListAsync();

            return dbList.Adapt<List<Trigger>>();
        }

        public async Task<List<Trigger>> GetByCommunicationModeIdAsync(int communicationModeId)
        {
            var dbList = await context.Triggers
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
            var dbList = await context.Triggers
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
            context.Triggers.Add(db);
            await context.SaveChangesAsync();

            // Reload with navigation properties
            var reloaded = await GetByIdAsync(db.Id);
            return reloaded!;
        }

        public async Task<bool> UpdateAsync(Trigger trigger)
        {
            var db = await context.Triggers.FirstOrDefaultAsync(t => t.Id == trigger.Id);
            if (db == null) return false;

            db.AlarmId = trigger.AlarmId;
            db.TriggerTypeId = trigger.TriggerTypeId;
            db.TriggerValue = trigger.TriggerValue;
            db.CommunicationModeId = trigger.CommunicationModeId;
            db.Subject = trigger.Subject;
            db.Body = trigger.Body;
            db.MinimumSendIntervalMinutes = trigger.MinimumSendIntervalMinutes;
            db.IsEnabled = trigger.IsEnabled;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var db = await context.Triggers.FirstOrDefaultAsync(t => t.Id == id);
            if (db == null) return false;

            context.Triggers.Remove(db);
            await context.SaveChangesAsync();
            return true;
        }

        // sTriggerTypeId: the TriggerTypeId for 'S'
        public async Task<List<BreachedTriggerDto>> GetNotReportedBreachesAsync()
        {
            var now = DateTime.UtcNow;

            var query =
                from sensor in context.Sensors
                join alarm in context.Alarms on sensor.AlarmId equals alarm.Id
                join trigger in context.Triggers on alarm.Id equals trigger.AlarmId
                join mostRecent in context.MostRecentReadings on sensor.Id equals mostRecent.SensorId into readingJoin
                from mostRecent in readingJoin.DefaultIfEmpty()
                where trigger.IsEnabled
                    && trigger.TriggerType.Code == TriggerTypeCode.NotReportedForPeriod
                    && (
                        mostRecent == null ||
                        now.AddMinutes(trigger.TriggerValue * -1) > mostRecent.DateRecordedUtc
                    )
                select new BreachedTriggerDto
                {
                    SensorId = sensor.Id,
                    TriggerTypeId = trigger.TriggerTypeId,
                    TriggerValue = trigger.TriggerValue,
                    CommunicationModeId = trigger.CommunicationModeId,
                    Subject = trigger.Subject,
                    Body = trigger.Body,
                    Value = mostRecent.Value.HasValue?mostRecent.Value.Value:0,
                    IsAlarm = false, // MostRecentReadings.IsAlarm not present in schema, set as null or add if available
                    DateRecordedUtc = mostRecent.DateRecordedUtc,
                    PendingAlarmTriggerId = null, // No equivalent in EF, set as null
                    AlarmSetId = alarm.Id, // Alarm is AlarmSet in this mapping
                    TriggerId = trigger.Id,
                    MinimumSendIntervalMinutes = trigger.MinimumSendIntervalMinutes
                };

            return await query.ToListAsync();
        }

        /// <returns>true if this alarm has already been sent within its quench period; false otherwise</returns>
        public async Task<bool> IsQuenchedAsync(BreachedTriggerDto trigger)
        {
            DateTime? mostRecentAlarm = await context.MostRecentAlarms
                .Where(mra => mra.SensorId == trigger.SensorId && mra.AlarmId == trigger.AlarmSetId)
                .Select(mra => mra.MostRecentSendUtc)
                .FirstOrDefaultAsync();

            if (mostRecentAlarm.HasValue)
            {
                var minutesSinceLastSend = (DateTime.UtcNow - mostRecentAlarm.Value).TotalMinutes;
                logger.LogDebug("Is quenched? {MinutesSinceLastSend} {MinimumSendIntervalMinutes}", 
                    minutesSinceLastSend, trigger.MinimumSendIntervalMinutes);
                return minutesSinceLastSend < trigger.MinimumSendIntervalMinutes;
            }

            logger.LogDebug("No previous alarm, so not quenched");
            return false;
        }
    }
}
