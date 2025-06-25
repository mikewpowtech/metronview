using Application.Dashboard;
using Domain;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DashboardRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<UnitSummary>> GetUnitSummaryAsync()
        {
            // Fetch units with related company, sensors, and readings
            var units = await _context.Units
                .Include(u => u.Company)
                .Include(u => u.Sensors)
                .Include(u => u.Readings)
                .Include(u => u.UnitType)
                .ToListAsync();

            var dashboards = units.Select(unit => new UnitSummary
            {
                Id = unit.Id,
                UnitType = unit.UnitType?.Name ?? "unknown", // Adjust as needed to get the name
                LastComms = unit.Readings.OrderByDescending(r => r.DateReceivedUtc).FirstOrDefault()?.DateReceivedUtc,
                //Alarm = unit.Sensors.Any(s => s.Name?.ToLower().Contains("alarm") == true),
                //Readings = unit.Readings.ToList(),
                //Sensors = unit.Sensors.ToList(),
                //AmbientTemperature = unit.Readings.OrderByDescending(r => r.DateReceivedUtc).FirstOrDefault()?.Value, // Example: Value1 as temperature
                Carrier = "EE", // Set if you have this info
                Signal = 27,  // Set if you have this info
                //Latitude = unit.Readings.OrderByDescending(r => r.DateReceivedUtc).FirstOrDefault()?.Value, // Example: Value1 as latitude
                //Longitude = unit.Readings.OrderByDescending(r => r.DateReceivedUtc).FirstOrDefault()?.Value, // Example: Value2 as longitude
                CompanyID = unit.CompanyID,
                Company = unit.Company?.Name ?? "unknown"
            }).ToList();

            return dashboards;
        }
    }
}