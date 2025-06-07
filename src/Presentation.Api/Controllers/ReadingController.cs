using Application.Readings;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReadingController : ControllerBase
{
    private readonly IReadingService _readingService;

    public ReadingController(IReadingService readingService)
    {
        _readingService = readingService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reading>>> GetReadings()
    {
        var readings = await _readingService.GetAllAsync();
        return Ok(readings);
    }

    [HttpGet("{dateRecordedUtc}/{sensorId:int}")]
    public async Task<ActionResult<Reading>> GetReading(DateTime dateRecordedUtc, int sensorId)
    {
        var reading = await _readingService.GetByIdAsync(dateRecordedUtc, sensorId);
        if (reading == null)
            return NotFound();
        return Ok(reading);
    }

    [HttpPost]
    public async Task<ActionResult<Reading>> AddReading([FromBody] Reading reading)
    {
        var createdReading = await _readingService.AddAsync(reading);
        return CreatedAtAction(nameof(GetReading), new { dateRecordedUtc = createdReading.DateRecordedUtc, sensorId = createdReading.Sensor.Id }, createdReading);
    }

    [HttpPut("{dateRecordedUtc}/{sensorId:int}")]
    public async Task<IActionResult> UpdateReading(DateTime dateRecordedUtc, int sensorId, [FromBody] Reading reading)
    {
        if (dateRecordedUtc != reading.DateRecordedUtc || sensorId != reading.Sensor.Id)
            return BadRequest("Key mismatch.");

        var success = await _readingService.UpdateAsync(reading);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{dateRecordedUtc}/{sensorId:int}")]
    public async Task<IActionResult> DeleteReading(DateTime dateRecordedUtc, int sensorId)
    {
        var success = await _readingService.DeleteAsync(dateRecordedUtc, sensorId);
        if (!success)
            return NotFound();

        return NoContent();
    }
}