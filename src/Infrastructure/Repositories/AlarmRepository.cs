using Application.Alarms;
using Domain;
using Infrastructure.DbClasses;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AlarmRepository : IAlarmRepository
    {
        private readonly ApplicationDbContext context;

        public AlarmRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Alarm?> GetByIdAsync(int id)
        {
            var db = await context.Alarms
                .Include(a => a.Triggers)
                .ThenInclude(t => t.TriggerType)
                .Include(a => a.Triggers)
                .ThenInclude(t => t.CommunicationMode)
                .Include(a => a.Company)
                .Include(a => a.RecipientSet)
                .FirstOrDefaultAsync(a => a.Id == id);

            return db?.Adapt<Alarm>();
        }

        public async Task<List<Alarm>> GetAllAsync()
        {
            var dbList = await context.Alarms
                .ToListAsync();

            // This line is now safe from circular references due to the configured mapping
            var response = dbList.Adapt<List<Alarm>>();
            return response;
        }

        public async Task<Alarm> AddAsync(Alarm alarm)
        {
            var db = alarm.Adapt<AlarmDb>();
            
            // Ensure proper parent-child relationships
            foreach (var trigger in db.Triggers)
            {
                trigger.Alarm = db;
                trigger.AlarmId = db.Id;
            }

            context.Alarms.Add(db);
            await context.SaveChangesAsync();
            
            // Return the fully populated alarm
            return await GetByIdAsync(db.Id) ?? alarm;
        }

        public async Task<bool> UpdateAsync(Alarm alarm)
        {
            var db = await context.Alarms
                .Include(a => a.Triggers)
                .FirstOrDefaultAsync(a => a.Id == alarm.Id);
            
            if (db == null) return false;

            // Update scalar properties
            db.Name = alarm.Name;
            db.CompanyId = alarm.CompanyId;
            db.RecipientSetId = alarm.RecipientSetId;
            db.IsActive = alarm.IsActive;

            // Handle triggers separately to avoid circular reference issues
            // Remove triggers that are not in the update
            var existingTriggerIds = db.Triggers.Select(t => t.Id).ToList();
            var newTriggerIds = alarm.Triggers.Select(t => t.Id).ToList();
            var triggersToRemove = db.Triggers.Where(t => !newTriggerIds.Contains(t.Id)).ToList();
            
            foreach (var triggerToRemove in triggersToRemove)
            {
                db.Triggers.Remove(triggerToRemove);
            }

            // Update or add triggers
            foreach (var trigger in alarm.Triggers)
            {
                var existingTrigger = db.Triggers.FirstOrDefault(t => t.Id == trigger.Id);
                if (existingTrigger != null)
                {
                    // Update existing trigger - use TriggerTypeId directly
                    existingTrigger.TriggerTypeId = trigger.TriggerTypeId;
                    existingTrigger.TriggerValue = trigger.TriggerValue;
                    existingTrigger.CommunicationModeId = trigger.CommunicationModeId;
                    existingTrigger.Subject = trigger.Subject;
                    existingTrigger.Body = trigger.Body;
                    existingTrigger.MinimumSendIntervalMinutes = trigger.MinimumSendIntervalMinutes;
                    existingTrigger.IsEnabled = trigger.IsEnabled;
                }
                else
                {
                    // Add new trigger - use TriggerTypeId directly
                    var newTrigger = new TriggerDb
                    {
                        AlarmId = db.Id,
                        TriggerTypeId = trigger.TriggerTypeId,
                        TriggerValue = trigger.TriggerValue,
                        CommunicationModeId = trigger.CommunicationModeId,
                        Subject = trigger.Subject,
                        Body = trigger.Body,
                        MinimumSendIntervalMinutes = trigger.MinimumSendIntervalMinutes,
                        IsEnabled = trigger.IsEnabled,
                        Alarm = db
                    };
                    db.Triggers.Add(newTrigger);
                }
            }

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var db = await context.Alarms
                .Include(a => a.Triggers)
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (db == null) return false;
            
            // EF Core will handle cascade delete for triggers based on the configuration
            context.Alarms.Remove(db);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<RecipientSet?> GetRecipientSetByAlarmIdAsync(int alarmId)
        {
            var recipientSetEntity = await context.Alarms
                .AsNoTracking()
                .Where(a => a.Id == alarmId)
                .Include(a => a.RecipientSet)
                    .ThenInclude(rs => rs.Recipients)
                .Select(a => a.RecipientSet)
                .FirstOrDefaultAsync();

            return recipientSetEntity?.Adapt<RecipientSet>();
        }

        public async Task<List<Alarm>> GetAlarmsByRecipientSetIdAsync(int recipientSetId)
        {
            var dbList = await context.Alarms
                .Where(a => a.RecipientSetId == recipientSetId)
                .ToListAsync();

            return dbList.Adapt<List<Alarm>>();
        }
    }
}