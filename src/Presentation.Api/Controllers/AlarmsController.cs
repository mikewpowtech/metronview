using Application.Alarms;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Presentation.Api.Models;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlarmsController : ControllerBase
{
    private readonly IAlarmService alarmService;

    public AlarmsController(IAlarmService alarmService)
    {
        this.alarmService = alarmService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Alarm?>> GetById(int id)
    {
        var alarm = await alarmService.GetByIdAsync(id);
        if (alarm == null)
            return NotFound();
        return Ok(alarm);
    }

    [HttpGet]
    public async Task<ActionResult<List<Alarm>>> GetAll([FromQuery] int? recipientSetId = null)
    {
        List<Alarm> alarms;
        
        if (recipientSetId.HasValue)
        {
            alarms = await alarmService.GetAlarmsByRecipientSetIdAsync(recipientSetId.Value);
        }
        else
        {
            alarms = await alarmService.GetAllAsync();
        }
        
        return Ok(alarms);
    }

    [HttpGet("{alarmId}/recipientset")]
    public async Task<ActionResult<RecipientSet>> GetAlarmRecipientSets(int alarmId)
    {
        try
        {
            var recipientSets = await alarmService.GetRecipientSetByAlarmIdAsync(alarmId);
            return Ok(recipientSets);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to fetch recipient set for alarm {alarmId}: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Alarm>> Add([FromBody] AlarmDto alarmDto)
    {
        var alarm = new Alarm
        {
            CompanyId = alarmDto.CompanyId,
            Name = alarmDto.Name,
            RecipientSetId = alarmDto.RecipientSetId,
            IsActive = alarmDto.IsActive
        };
        
        var created = await alarmService.AddAsync(alarm);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] AlarmDto alarmDto)
    {
        if (id != alarmDto.Id)
            return BadRequest("ID mismatch");

        var alarm = new Alarm
        {
            Id = alarmDto.Id,
            CompanyId = alarmDto.CompanyId,
            Name = alarmDto.Name,
            RecipientSetId = alarmDto.RecipientSetId,
            IsActive = alarmDto.IsActive
        };

        var updated = await alarmService.UpdateAsync(alarm);
        if (!updated)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await alarmService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
