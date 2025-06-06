using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CompaniesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
    {
        var companies = await _context.Companies.ToListAsync();
        return Ok(companies);
    }

    [HttpPost]
    public async Task<ActionResult<Company>> AddCompany([FromBody] Company company)
    {
        if (string.IsNullOrWhiteSpace(company.Name))
        {
            return BadRequest("Company name is required.");
        }

        company.Id = Guid.NewGuid().ToString();
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCompanies), new { id = company.Id }, company);
    }
}