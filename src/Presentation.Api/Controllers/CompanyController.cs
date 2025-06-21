using Application.Companies;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
    {
        var companies = await _companyService.GetAllAsync();
        return Ok(companies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Company>> GetCompany(int id)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null)
            return NotFound();
        return Ok(company);
    }

    [HttpPost]
    public async Task<ActionResult<Company>> AddCompany([FromBody] Company company)
    {
        if (string.IsNullOrWhiteSpace(company.Name))
        {
            return BadRequest("Company name is required.");
        }

        var createdCompany = await _companyService.AddAsync(company);
        return CreatedAtAction(nameof(GetCompany), new { id = createdCompany.Id }, createdCompany);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, [FromBody] Company company)
    {
        if (id != company.Id)
            return BadRequest("ID mismatch.");

        var success = await _companyService.UpdateAsync(company);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var success = await _companyService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}