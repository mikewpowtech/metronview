using Application.Recipients;
using Domain;
using Infrastructure.DbClasses;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RecipientRepository : IRecipientRepository
    {
        private readonly ApplicationDbContext _context;

        public RecipientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Recipient?> GetByIdAsync(int id)
        {
            var db = await _context.Set<RecipientDb>().FirstOrDefaultAsync(r => r.Id == id);
            return db == null ? null : db.Adapt<Recipient>();
        }

        public async Task<List<Recipient>> GetAllAsync()
        {
            var dbList = await _context.Set<RecipientDb>().ToListAsync();
            return dbList.Adapt<List<Recipient>>();
        }

        public async Task<Recipient> AddAsync(Recipient recipient)
        {
            var db = recipient.Adapt<RecipientDb>();
            _context.Set<RecipientDb>().Add(db);
            await _context.SaveChangesAsync();
            return db.Adapt<Recipient>();
        }

        public async Task<bool> UpdateAsync(Recipient recipient)
        {
            var db = await _context.Set<RecipientDb>().FindAsync(recipient.Id);
            if (db == null) return false;
            recipient.Adapt(db);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var db = await _context.Set<RecipientDb>().FindAsync(id);
            if (db == null) return false;
            _context.Set<RecipientDb>().Remove(db);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}