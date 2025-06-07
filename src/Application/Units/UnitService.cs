using Domain;

namespace Application.Units;

public class UnitService : IUnitService
{
    private readonly IUnitRepository _unitRepository;

    public UnitService(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public Task<Unit?> GetByIdAsync(string id) => _unitRepository.GetByIdAsync(id);

    public Task<List<Unit>> GetAllAsync() => _unitRepository.GetAllAsync();

    public Task<Unit> AddAsync(Unit unit) => _unitRepository.AddAsync(unit);

    public Task<bool> UpdateAsync(Unit unit) => _unitRepository.UpdateAsync(unit);

    public Task<bool> DeleteAsync(string id) => _unitRepository.DeleteAsync(id);
}