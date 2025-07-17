using Application.CustomFields.Dtos;
using Domain;
using Domain.Enums;

namespace Application.CustomFields;
public class CustomFieldService : ICustomFieldService
{
    private readonly ICustomFieldRepository _repository;
        
    public CustomFieldService(ICustomFieldRepository repository)
    {
        _repository = repository;
    }

    public Task<CustomField?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<List<CustomField>> GetAllAsync() => _repository.GetAllAsync();
    public Task<List<CustomField>> GetByForeignKeyAsync(int foreignKeyId, CustomFieldType fieldType) => _repository.GetByForeignKeyAsync(foreignKeyId, fieldType);
    public Task<CustomField> AddAsync(CustomField customField) => _repository.AddAsync(customField);
    public Task<bool> UpdateAsync(CustomField customField) => _repository.UpdateAsync(customField);
    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
    public Task<bool> DeleteByForeignKeyAsync(int foreignKeyId, CustomFieldType fieldType) => _repository.DeleteByForeignKeyAsync(foreignKeyId, fieldType);

    public IList<HenkelRtuStatus> GetEnabledCustomFieldValues(string enabledFieldName, string valueFieldsName)
    {
        throw new NotImplementedException();
    }

    public void SetHenkelStatusCustomFieldValueCache(HenkelRtuStatus enabledRtu, HenkelStatusType status, string henkelSpiderScopeStatusCustomFieldName)
    {
        throw new NotImplementedException();
    }
}