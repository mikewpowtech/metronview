using Application.CommunicationModes;
using Domain;
using Infrastructure.DbClasses;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CommunicationModeRepository : ICommunicationModeRepository
    {
        private readonly ApplicationDbContext _context;

        public CommunicationModeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CommunicationMode?> GetByIdAsync(int id)
        {
            var db = await _context.RecipientModes
                .FirstOrDefaultAsync(cm => cm.Id == id);

            return db == null ? null : db.Adapt<CommunicationMode>();
        }

        public async Task<CommunicationMode?> GetByCodeAsync(string code)
        {
            var db = await _context.RecipientModes
                .FirstOrDefaultAsync(cm => cm.Code == code);

            return db == null ? null : db.Adapt<CommunicationMode>();
        }

        public async Task<List<CommunicationMode>> GetAllAsync()
        {
            var dbList = await _context.RecipientModes
                .OrderBy(cm => cm.Order)
                .ToListAsync();

            return dbList.Adapt<List<CommunicationMode>>();
        }

        public async Task<CommunicationMode> AddAsync(CommunicationMode communicationMode)
        {
            var db = communicationMode.Adapt<CommunicationModeDb>();
            _context.RecipientModes.Add(db);
            await _context.SaveChangesAsync();
            return db.Adapt<CommunicationMode>();
        }

        public async Task<bool> UpdateAsync(CommunicationMode communicationMode)
        {
            var db = await _context.RecipientModes.FirstOrDefaultAsync(cm => cm.Id == communicationMode.Id);
            if (db == null) return false;

            db.Code = communicationMode.Code;
            db.Name = communicationMode.Name;
            db.Order = communicationMode.Order;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var db = await _context.RecipientModes.FirstOrDefaultAsync(cm => cm.Id == id);
            if (db == null) return false;

            _context.RecipientModes.Remove(db);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
