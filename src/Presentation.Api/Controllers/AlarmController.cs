using Application.Alarms;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Presentation.Api.Models;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlarmController : ControllerBase
{
    private readonly IAlarmService _alarmService;

    public AlarmController(IAlarmService alarmService)
    {
        _alarmService = alarmService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Alarm?>> GetById(int id)
    {
        var alarm = await _alarmService.GetByIdAsync(id);
        if (alarm == null)
            return NotFound();
        return Ok(alarm);
    }

    [HttpGet]
    public async Task<ActionResult<List<Alarm>>> GetAll()
    {
        var alarms = await _alarmService.GetAllAsync();
        return Ok(alarms);
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
        
        var created = await _alarmService.AddAsync(alarm);
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

        var updated = await _alarmService.UpdateAsync(alarm);
        if (!updated)
            return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _alarmService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
