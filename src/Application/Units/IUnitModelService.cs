using Domain;

namespace Application.Units;

public interface IUnitModelService
{
    Task<UnitModel?> GetByIdAsync(int id);
    Task<List<UnitModel>> GetAllAsync();
    Task<UnitModel> AddAsync(UnitModel unitModel);
    Task<bool> UpdateAsync(UnitModel unitModel);
    Task<bool> DeleteAsync(int id);
}