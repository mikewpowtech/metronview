using Application.Triggers;
using Domain;
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
                .Include(t => t.Alarm)
                .Include(t => t.TriggerType)
                .Include(t => t.CommunicationMode)
                .FirstOrDefaultAsync(t => t.Id == id);

            return db == null ? null : db.Adapt<Trigger>();
        }

        public async Task<List<Trigger>> GetAllAsync()
        {
            var dbList = await _context.Triggers
                .Include(t => t.Alarm)
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

        public async Task<List<Trigger>> GetByTriggerTypeIdAsync(int triggerTypeId)
        {
            var dbList = await _context.Triggers
                .Include(t => t.Alarm)
                .Include(t => t.TriggerType)
                .Include(t => t.CommunicationMode)
                .Where(t => t.TriggerTypeId == triggerTypeId)
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
            db.TriggerTypeId = trigger.TriggerTypeId;
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
    }
}
