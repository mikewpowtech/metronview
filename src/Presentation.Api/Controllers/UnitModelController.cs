using Application.Units;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitModelController : ControllerBase
{
    private readonly IUnitModelService _unitModelService;

    public UnitModelController(IUnitModelService unitModelService)
    {
        _unitModelService = unitModelService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UnitModel>>> GetUnitModels()
    {
        var models = await _unitModelService.GetAllAsync();
        return Ok(models);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UnitModel>> GetUnitModel(int id)
    {
        var model = await _unitModelService.GetByIdAsync(id);
        if (model == null)
            return NotFound();
        return Ok(model);
    }

    [HttpPost]
    public async Task<ActionResult<UnitModel>> AddUnitModel([FromBody] UnitModel unitModel)
    {
        if (string.IsNullOrWhiteSpace(unitModel.Code) || string.IsNullOrWhiteSpace(unitModel.Name))
        {
            return BadRequest("Code and Name are required.");
        }

        var created = await _unitModelService.AddAsync(unitModel);
        return CreatedAtAction(nameof(GetUnitModel), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUnitModel(int id, [FromBody] UnitModel unitModel)
    {
        if (id != unitModel.Id)
            return BadRequest("ID mismatch.");

        var success = await _unitModelService.UpdateAsync(unitModel);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUnitModel(int id)
    {
        var success = await _unitModelService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}