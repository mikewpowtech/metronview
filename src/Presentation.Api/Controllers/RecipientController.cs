using Application.Recipients;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipientController : ControllerBase
{
    private readonly IRecipientService _recipientService;

    public RecipientController(IRecipientService recipientService)
    {
        _recipientService = recipientService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Recipient?>> GetById(int id)
    {
        var recipient = await _recipientService.GetByIdAsync(id);
        if (recipient == null)
            return NotFound();
        return Ok(recipient);
    }

    [HttpGet]
    public async Task<ActionResult<List<Recipient>>> GetAll()
    {
        var recipients = await _recipientService.GetAllAsync();
        return Ok(recipients);
    }

    [HttpPost]
    public async Task<ActionResult<Recipient>> Add([FromBody] Recipient recipient)
    {
        var created = await _recipientService.AddAsync(recipient);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] Recipient recipient)
    {
        if (id != recipient.Id)
            return BadRequest("ID mismatch");

        var updated = await _recipientService.UpdateAsync(recipient);
        if (!updated)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _recipientService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
