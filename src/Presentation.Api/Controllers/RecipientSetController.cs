using Application.Alarms;
using Application.RecipientSets;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipientSetController : ControllerBase
{
    private readonly IRecipientSetService recipientSetService;
    private readonly IAlarmService alarmsService;

    public RecipientSetController(IRecipientSetService recipientSetService, IAlarmService alarmsService)
    {
        this.recipientSetService = recipientSetService;
        this.alarmsService = alarmsService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecipientSet?>> GetById(int id)
    {
        var recipientSet = await recipientSetService.GetByIdAsync(id);
        if (recipientSet == null)
            return NotFound();
        return Ok(recipientSet);
    }

    [HttpGet("{recipientSetId}/alarms")]
    public async Task<ActionResult<List<Alarm>>> GetAlarmsByRecipientSet(int recipientSetId)
    {
        var alarms = await alarmsService.GetAlarmsByRecipientSetIdAsync(recipientSetId);
        return Ok(alarms);
    }

    [HttpGet("{recipientSetId}/recipients")]
    public async Task<ActionResult<IEnumerable<Recipient>>> GetRecipientSetRecipients(int recipientSetId)
    {
        try
        {
            var recipients = await recipientSetService.GetRecipientsByRecipientSetIdAsync(recipientSetId);
            return Ok(recipients);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to fetch recipients for recipient set {recipientSetId}: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<RecipientSet>>> GetAll()
    {
        var recipientSets = await recipientSetService.GetAllAsync();
        return Ok(recipientSets);
    }

    [HttpPost]
    public async Task<ActionResult<RecipientSet>> Add([FromBody] RecipientSet recipientSet)
    {
        var created = await recipientSetService.AddAsync(recipientSet);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] RecipientSet recipientSet)
    {
        if (id != recipientSet.Id)
            return BadRequest("ID mismatch");

        var updated = await recipientSetService.UpdateAsync(recipientSet);
        if (!updated)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await recipientSetService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}