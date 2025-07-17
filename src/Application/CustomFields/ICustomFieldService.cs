using Application.CustomFields.Dtos;
using Domain;
using Domain.Enums;

namespace Application.CustomFields;

public interface ICustomFieldService
{
    Task<CustomField?> GetByIdAsync(int id);
    Task<List<CustomField>> GetAllAsync();
    Task<List<CustomField>> GetByForeignKeyAsync(int foreignKeyId, CustomFieldType fieldType);
    Task<CustomField> AddAsync(CustomField customField);
    Task<bool> UpdateAsync(CustomField customField);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteByForeignKeyAsync(int foreignKeyId, CustomFieldType fieldType);
    IList<HenkelRtuStatus> GetEnabledCustomFieldValues(string enabledFieldName, string valueFieldsName);
    void SetHenkelStatusCustomFieldValueCache(HenkelRtuStatus enabledRtu, HenkelStatusType status, string henkelSpiderScopeStatusCustomFieldName);
}