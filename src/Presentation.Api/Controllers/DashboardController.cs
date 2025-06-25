using Application.Dashboard;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<ActionResult<UnitSummary>> Get()
        {
            var unitsummaries = await _dashboardService.GetDashboardAsync();
            return Ok(unitsummaries);
        }
    }
}