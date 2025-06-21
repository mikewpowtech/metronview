using Domain;

namespace Application.Companies;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repository;

    public CompanyService(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public Task<Company?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task<List<Company>> GetAllAsync() => _repository.GetAllAsync();

    public Task<Company> AddAsync(Company company) => _repository.AddAsync(company);

    public Task<bool> UpdateAsync(Company company) => _repository.UpdateAsync(company);

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
}