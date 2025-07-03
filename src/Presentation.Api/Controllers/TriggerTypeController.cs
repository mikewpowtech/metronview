using Microsoft.AspNetCore.Mvc;
using Application.TriggerTypes;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TriggerTypeController : ControllerBase
    {
        private readonly ITriggerTypeService _triggerTypeService;

        public TriggerTypeController(ITriggerTypeService triggerTypeService)
        {
            _triggerTypeService = triggerTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var triggerTypes = await _triggerTypeService.GetAllAsync();
                return Ok(triggerTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve trigger types", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var triggerType = await _triggerTypeService.GetByIdAsync(id);

                if (triggerType == null)
                {
                    return NotFound(new { error = "Trigger type not found" });
                }

                return Ok(triggerType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve trigger type", details = ex.Message });
            }
        }

        [HttpGet("by-code/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            try
            {
                var triggerType = await _triggerTypeService.GetByCodeAsync(code);

                if (triggerType == null)
                {
                    return NotFound(new { error = "Trigger type not found" });
                }

                return Ok(triggerType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve trigger type", details = ex.Message });
            }
        }
    }
}
