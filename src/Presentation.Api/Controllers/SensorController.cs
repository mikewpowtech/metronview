using Application.Sensors;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SensorController(ISensorService sensorService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sensor>>> GetSensors()
    {
        var sensors = await sensorService.GetAllAsync();
        return Ok(sensors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Sensor>> GetSensor(int id)
    {
        var sensor = await sensorService.GetByIdAsync(id);
        if (sensor == null)
            return NotFound();
        return Ok(sensor);
    }

    [HttpGet("by-unit/{unitId}")]
    public async Task<IActionResult> GetSensorsByUnit(int unitId)
    {
        var sensors = await sensorService.GetByUnitIdAsync(unitId);
        return Ok(sensors);
    }

    [HttpPost]
    public async Task<ActionResult<Sensor>> AddSensor([FromBody] Sensor sensor)
    {
        // Add any required validation here if needed
        var createdSensor = await sensorService.AddAsync(sensor);
        return CreatedAtAction(nameof(GetSensor), new { id = createdSensor.Id }, createdSensor);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSensor(int id, [FromBody] Sensor sensor)
    {
        if (id != sensor.Id)
            return BadRequest("ID mismatch.");

        var success = await sensorService.UpdateAsync(sensor);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSensor(int id)
    {
        var success = await sensorService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}