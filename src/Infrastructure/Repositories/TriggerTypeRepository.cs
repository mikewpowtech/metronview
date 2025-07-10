using Application.TriggerTypes;
using Domain;
using Domain.Enums;
using Infrastructure.DbClasses;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TriggerTypeRepository : ITriggerTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public TriggerTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TriggerType?> GetByIdAsync(int id)
        {
            var db = await _context.TriggerTypes
                .FirstOrDefaultAsync(tt => tt.Id == id);

            return db == null ? null : db.Adapt<TriggerType>();
        }


        public async Task<TriggerType?> GetByCodeAsync(TriggerTypeCode code)
        {
            var db = await _context.TriggerTypes
                .FirstOrDefaultAsync(tt => tt.Code == code);

            return db == null ? null : db.Adapt<TriggerType>();
        }

        public async Task<List<TriggerType>> GetAllAsync()
        {
            var dbList = await _context.TriggerTypes
                .OrderBy(tt => tt.Order)
                .ToListAsync();

            return dbList.Adapt<List<TriggerType>>();
        }

        public async Task<TriggerType> AddAsync(TriggerType triggerType)
        {
            var db = triggerType.Adapt<TriggerTypeDb>();
            _context.TriggerTypes.Add(db);
            await _context.SaveChangesAsync();
            return db.Adapt<TriggerType>();
        }

        public async Task<bool> UpdateAsync(TriggerType triggerType)
        {
            var db = await _context.TriggerTypes.FirstOrDefaultAsync(tt => tt.Id == triggerType.Id);
            if (db == null) return false;

            db.Code = triggerType.Code;
            db.Name = triggerType.Name;
            db.Order = triggerType.Order;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var db = await _context.TriggerTypes.FirstOrDefaultAsync(tt => tt.Id == id);
            if (db == null) return false;

            _context.TriggerTypes.Remove(db);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
