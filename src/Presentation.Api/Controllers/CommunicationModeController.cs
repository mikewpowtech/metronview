using Microsoft.AspNetCore.Mvc;
using Application.CommunicationModes;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunicationModeController : ControllerBase
    {
        private readonly ICommunicationModeService _communicationModeService;

        public CommunicationModeController(ICommunicationModeService communicationModeService)
        {
            _communicationModeService = communicationModeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var communicationModes = await _communicationModeService.GetAllAsync();
                return Ok(communicationModes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve communication modes", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var communicationMode = await _communicationModeService.GetByIdAsync(id);

                if (communicationMode == null)
                {
                    return NotFound(new { error = "Communication mode not found" });
                }

                return Ok(communicationMode);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve communication mode", details = ex.Message });
            }
        }

        [HttpGet("by-code/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            try
            {
                var communicationMode = await _communicationModeService.GetByCodeAsync(code);

                if (communicationMode == null)
                {
                    return NotFound(new { error = "Communication mode not found" });
                }

                return Ok(communicationMode);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve communication mode", details = ex.Message });
            }
        }
    }
}
