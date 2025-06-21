using Application.Units;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitController : ControllerBase
{
    private readonly IUnitService _unitService;

    public UnitController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Unit>>> GetUnits()
    {
        var units = await _unitService.GetAllAsync();
        return Ok(units);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Unit>> GetUnit(int id)
    {
        var unit = await _unitService.GetByIdAsync(id);
        if (unit == null)
            return NotFound();
        return Ok(unit);
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> AddUnit([FromBody] Unit unit)
    {
        if (string.IsNullOrWhiteSpace(unit.UnitTypeId) || string.IsNullOrWhiteSpace(unit.ManufacturerCode))
        {
            return BadRequest("Required fields are missing.");
        }

        var createdUnit = await _unitService.AddAsync(unit);
        return CreatedAtAction(nameof(GetUnit), new { id = createdUnit.Id }, createdUnit);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUnit(int id, [FromBody] Unit unit)
    {
        if (id != unit.Id)
            return BadRequest("ID mismatch.");

        var success = await _unitService.UpdateAsync(unit);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUnit(int id)
    {
        var success = await _unitService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}