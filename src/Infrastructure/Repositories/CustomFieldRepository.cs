using Application.CustomFields;
using Domain;
using Domain.Enums;
using Infrastructure.DbClasses;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CustomFieldRepository : ICustomFieldRepository
{
    private readonly ApplicationDbContext _context;

    public CustomFieldRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomField?> GetByIdAsync(int id)
    {
        var db = await _context.CustomFields
            .FirstOrDefaultAsync(cf => cf.Id == id);
        return db?.Adapt<CustomField>();
    }

    public async Task<List<CustomField>> GetAllAsync()
    {
        var dbList = await _context.CustomFields
            .OrderBy(cf => cf.CustomFieldType)
            .ThenBy(cf => cf.ForeignKeyId)
            .ThenBy(cf => cf.Title)
            .ToListAsync();
        return dbList.Adapt<List<CustomField>>();
    }

    public async Task<List<CustomField>> GetByForeignKeyAsync(int foreignKeyId, CustomFieldType fieldType)
    {
        var dbList = await _context.CustomFields
            .Where(cf => cf.ForeignKeyId == foreignKeyId && cf.CustomFieldType == fieldType)
            .OrderBy(cf => cf.Title)
            .ToListAsync();
        return dbList.Adapt<List<CustomField>>();
    }

    public async Task<CustomField> AddAsync(CustomField customField)
    {
        var db = customField.Adapt<CustomFieldDb>();
        _context.CustomFields.Add(db);
        await _context.SaveChangesAsync();
        return db.Adapt<CustomField>();
    }

    public async Task<bool> UpdateAsync(CustomField customField)
    {
        var db = await _context.CustomFields.FindAsync(customField.Id);
        if (db == null) return false;

        db.ForeignKeyId = customField.ForeignKeyId;
        db.Title = customField.Title;
        db.Content = customField.Content;
        db.CustomFieldType = customField.CustomFieldType;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var db = await _context.CustomFields.FindAsync(id);
        if (db == null) return false;

        _context.CustomFields.Remove(db);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByForeignKeyAsync(int foreignKeyId, CustomFieldType fieldType)
    {
        var dbList = await _context.CustomFields
            .Where(cf => cf.ForeignKeyId == foreignKeyId && cf.CustomFieldType == fieldType)
            .ToListAsync();

        if (dbList.Count == 0) return false;

        _context.CustomFields.RemoveRange(dbList);
        await _context.SaveChangesAsync();
        return true;
    }
}