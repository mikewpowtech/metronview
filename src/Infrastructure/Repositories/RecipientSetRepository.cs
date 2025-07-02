using Application.RecipientSets;
using Domain;
using Infrastructure.DbClasses;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RecipientSetRepository : IRecipientSetRepository
    {
        private readonly ApplicationDbContext _context;

        public RecipientSetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RecipientSet?> GetByIdAsync(int id)
        {
            var db = await _context.Set<RecipientSetDb>()
                .Include(rs => rs.Recipients)
                .FirstOrDefaultAsync(rs => rs.Id == id);
            return db == null ? null : db.Adapt<RecipientSet>();
        }

        public async Task<List<RecipientSet>> GetAllAsync()
        {
            var dbList = await _context.Set<RecipientSetDb>()
                .Include(rs => rs.Recipients)
                .ToListAsync();
            return dbList.Adapt<List<RecipientSet>>();
        }

        public async Task<RecipientSet> AddAsync(RecipientSet recipientSet)
        {
            var db = recipientSet.Adapt<RecipientSetDb>();
            _context.Set<RecipientSetDb>().Add(db);
            await _context.SaveChangesAsync();
            return db.Adapt<RecipientSet>();
        }

        public async Task<bool> UpdateAsync(RecipientSet recipientSet)
        {
            var db = await _context.Set<RecipientSetDb>().FindAsync(recipientSet.Id);
            if (db == null) return false;
            recipientSet.Adapt(db);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var db = await _context.Set<RecipientSetDb>().FindAsync(id);
            if (db == null) return false;
            _context.Set<RecipientSetDb>().Remove(db);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}