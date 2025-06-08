using Domain;

namespace Application.Units;

public class UnitModelService : IUnitModelService
{
    private readonly IUnitModelRepository _repository;

    public UnitModelService(IUnitModelRepository repository)
    {
        _repository = repository;
    }

    public Task<UnitModel?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task<List<UnitModel>> GetAllAsync() => _repository.GetAllAsync();

    public Task<UnitModel> AddAsync(UnitModel unitModel) => _repository.AddAsync(unitModel);

    public Task<bool> UpdateAsync(UnitModel unitModel) => _repository.UpdateAsync(unitModel);

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
}