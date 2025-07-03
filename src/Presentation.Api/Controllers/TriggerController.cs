using Microsoft.AspNetCore.Mvc;
using Application.Triggers;
using Domain;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TriggerController : ControllerBase
    {
        private readonly ITriggerService _triggerService;

        public TriggerController(ITriggerService triggerService)
        {
            _triggerService = triggerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var triggers = await _triggerService.GetAllAsync();
                return Ok(triggers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve triggers", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var trigger = await _triggerService.GetByIdAsync(id);

                if (trigger == null)
                {
                    return NotFound(new { error = "Trigger not found" });
                }

                return Ok(trigger);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve trigger", details = ex.Message });
            }
        }

        [HttpGet("by-alarm/{alarmId}")]
        public async Task<IActionResult> GetByAlarmId(int alarmId)
        {
            try
            {
                var triggers = await _triggerService.GetByAlarmIdAsync(alarmId);
                return Ok(triggers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve triggers by alarm", details = ex.Message });
            }
        }

        [HttpGet("by-trigger-type/{triggerTypeId}")]
        public async Task<IActionResult> GetByTriggerTypeId(int triggerTypeId)
        {
            try
            {
                var triggers = await _triggerService.GetByTriggerTypeIdAsync(triggerTypeId);
                return Ok(triggers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve triggers by trigger type", details = ex.Message });
            }
        }

        [HttpGet("by-communication-mode/{communicationModeId}")]
        public async Task<IActionResult> GetByCommunicationModeId(int communicationModeId)
        {
            try
            {
                var triggers = await _triggerService.GetByCommunicationModeIdAsync(communicationModeId);
                return Ok(triggers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve triggers by communication mode", details = ex.Message });
            }
        }

        [HttpGet("enabled")]
        public async Task<IActionResult> GetEnabledTriggers()
        {
            try
            {
                var triggers = await _triggerService.GetEnabledTriggersAsync();
                return Ok(triggers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve enabled triggers", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Trigger trigger)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdTrigger = await _triggerService.AddAsync(trigger);
                return CreatedAtAction(nameof(GetById), new { id = createdTrigger.Id }, createdTrigger);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create trigger", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Trigger trigger)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != trigger.Id)
                {
                    return BadRequest(new { error = "ID mismatch" });
                }

                var updated = await _triggerService.UpdateAsync(trigger);
                if (!updated)
                {
                    return NotFound(new { error = "Trigger not found" });
                }

                return Ok(new { message = "Trigger updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update trigger", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _triggerService.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new { error = "Trigger not found" });
                }

                return Ok(new { message = "Trigger deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to delete trigger", details = ex.Message });
            }
        }
    }
}
