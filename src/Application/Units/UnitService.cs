using Domain;

namespace Application.Units;

public class UnitService(IUnitRepository unitRepository) : IUnitService
{
    public Task<Unit?> GetByIdAsync(int id) => unitRepository.GetByIdAsync(id);

    public Task<List<Unit>> GetAllAsync() => unitRepository.GetAllAsync();

    public Task<Unit> AddAsync(Unit unit) => unitRepository.AddAsync(unit);

    public Task<bool> UpdateAsync(Unit unit) => unitRepository.UpdateAsync(unit);

    public Task<bool> DeleteAsync(int id) => unitRepository.DeleteAsync(id);
}