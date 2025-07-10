using Application.Alarms;
using Domain;
using Infrastructure.DbClasses;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AlarmRepository : IAlarmRepository
    {
        private readonly ApplicationDbContext _context;

        public AlarmRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Alarm?> GetByIdAsync(int id)
        {
            var db = await _context.Alarms
                .Include(a => a.Triggers)
                .FirstOrDefaultAsync(a => a.Id == id);

            return db == null ? null : db.Adapt<Alarm>();
        }

        public async Task<List<Alarm>> GetAllAsync()
        {
            var dbList = await _context.Alarms
                .Include(a => a.Triggers)
                .ThenInclude(t => t.TriggerType)
                .ToListAsync();

            return dbList.Adapt<List<Alarm>>();
        }

        public async Task<Alarm> AddAsync(Alarm alarm)
        {
            var db = alarm.Adapt<AlarmDb>();
            _context.Alarms.Add(db);
            await _context.SaveChangesAsync();
            return db.Adapt<Alarm>();
        }

        public async Task<bool> UpdateAsync(Alarm alarm)
        {
            var db = await _context.Alarms.FindAsync(alarm.Id);
            if (db == null) return false;

            alarm.Adapt(db); // Map updated fields onto the tracked entity
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var db = await _context.Alarms.FindAsync(id);
            if (db == null) return false;
            _context.Alarms.Remove(db);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}