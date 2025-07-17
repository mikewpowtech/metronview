using Application.Dashboard;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            this.dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<ActionResult<UnitSummary>> Get()
        {
            var unitsummaries = await dashboardService.GetDashboardAsync();
            return Ok(unitsummaries);
        }
    }
}