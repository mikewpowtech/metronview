using Application.Recipients;
using Application.Triggers;
using Domain;
using Infrastructure.DbClasses;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RecipientRepository(ApplicationDbContext context) : IRecipientRepository
    {
        public async Task<Recipient?> GetByIdAsync(int id)
        {
            var db = await context.Set<RecipientDb>().FirstOrDefaultAsync(r => r.Id == id);
            return db == null ? null : db.Adapt<Recipient>();
        }

        public async Task<List<Recipient>> GetAllAsync()
        {
            var dbList = await context.Set<RecipientDb>().ToListAsync();
            return dbList.Adapt<List<Recipient>>();
        }

        public async Task<Recipient> AddAsync(Recipient recipient)
        {
            var db = recipient.Adapt<RecipientDb>();
            context.Set<RecipientDb>().Add(db);
            await context.SaveChangesAsync();
            return db.Adapt<Recipient>();
        }

        public async Task<bool> UpdateAsync(Recipient recipient)
        {
            var db = await context.Set<RecipientDb>().FindAsync(recipient.Id);
            if (db == null) return false;
            recipient.Adapt(db);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var db = await context.Set<RecipientDb>().FindAsync(id);
            if (db == null) return false;
            context.Set<RecipientDb>().Remove(db);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<RecipientTemplates>> GetRecipientTemplatesAsync(BreachedTriggerDto triggerDto)
        {
            var query = from alarm in context.Alarms
                        join recipientSet in context.RecipientSets on alarm.RecipientSetId equals recipientSet.Id
                        from recipient in recipientSet.Recipients  // Use navigation property
                        join company in context.Companies on recipient.CompanyId equals company.Id
                        where alarm.Id == triggerDto.AlarmSetId
                              && recipient.Sms != null
                              && recipient.IsEnabled
                        select new RecipientTemplates
                        {
                            Sms = recipient.Sms,
                            Email = recipient.Email,
                            ToAddressTemplate = company.AlarmSmsToAddressTemplate,
                            SubjectTemplate = company.AlarmSmsSubjectTemplate,
                            BodyTemplate = company.AlarmSmsBodyTemplate,
                            FromAddress = company.AlarmEmailFromAddress,
                            ReplyToAddress = company.AlarmEmailReplyToAddress,
                            CompanyId = recipient.CompanyId,
                            RecipientId = recipient.Id
                        };

            return await query.ToListAsync();
        }
    }
}