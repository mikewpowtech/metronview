using Application.Sensors;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SensorController : ControllerBase
{
    private readonly ISensorService _sensorService;

    public SensorController(ISensorService sensorService)
    {
        _sensorService = sensorService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sensor>>> GetSensors()
    {
        var sensors = await _sensorService.GetAllAsync();
        return Ok(sensors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Sensor>> GetSensor(string id)
    {
        var sensor = await _sensorService.GetByIdAsync(id);
        if (sensor == null)
            return NotFound();
        return Ok(sensor);
    }

    [HttpPost]
    public async Task<ActionResult<Sensor>> AddSensor([FromBody] Sensor sensor)
    {
        // Add any required validation here if needed
        var createdSensor = await _sensorService.AddAsync(sensor);
        return CreatedAtAction(nameof(GetSensor), new { id = createdSensor.Id }, createdSensor);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSensor(string id, [FromBody] Sensor sensor)
    {
        if (id != sensor.Id)
            return BadRequest("ID mismatch.");

        var success = await _sensorService.UpdateAsync(sensor);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSensor(string id)
    {
        var success = await _sensorService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}