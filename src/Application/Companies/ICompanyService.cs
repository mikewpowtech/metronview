using Domain;

namespace Application.Companies;

public interface ICompanyService
{
    Task<Company?> GetByIdAsync(string id);
    Task<List<Company>> GetAllAsync();
    Task<Company> AddAsync(Company company);
    Task<bool> UpdateAsync(Company company);
    Task<bool> DeleteAsync(string id);
}