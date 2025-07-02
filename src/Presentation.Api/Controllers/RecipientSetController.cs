using Application.RecipientSets;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipientSetController : ControllerBase
{
    private readonly IRecipientSetService _recipientSetService;

    public RecipientSetController(IRecipientSetService recipientSetService)
    {
        _recipientSetService = recipientSetService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecipientSet?>> GetById(int id)
    {
        var recipientSet = await _recipientSetService.GetByIdAsync(id);
        if (recipientSet == null)
            return NotFound();
        return Ok(recipientSet);
    }

    [HttpGet]
    public async Task<ActionResult<List<RecipientSet>>> GetAll()
    {
        var recipientSets = await _recipientSetService.GetAllAsync();
        return Ok(recipientSets);
    }

    [HttpPost]
    public async Task<ActionResult<RecipientSet>> Add([FromBody] RecipientSet recipientSet)
    {
        var created = await _recipientSetService.AddAsync(recipientSet);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] RecipientSet recipientSet)
    {
        if (id != recipientSet.Id)
            return BadRequest("ID mismatch");

        var updated = await _recipientSetService.UpdateAsync(recipientSet);
        if (!updated)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _recipientSetService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}