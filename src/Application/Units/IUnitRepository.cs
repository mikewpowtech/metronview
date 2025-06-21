using Domain;

namespace Application.Units;

public interface IUnitRepository
{
    Task<Unit?> GetByIdAsync(int id);
    Task<List<Unit>> GetAllAsync();
    Task<Unit> AddAsync(Unit unit);
    Task<bool> UpdateAsync(Unit unit);
    Task<bool> DeleteAsync(int id);
}