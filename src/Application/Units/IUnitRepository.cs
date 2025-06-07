using Domain;

namespace Application.Units;

public interface IUnitRepository
{
    Task<Unit?> GetByIdAsync(string id);
    Task<List<Unit>> GetAllAsync();
    Task<Unit> AddAsync(Unit unit);
    Task<bool> UpdateAsync(Unit unit);
    Task<bool> DeleteAsync(string id);
}