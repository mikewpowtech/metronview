using Application.Dashboard;
using Domain;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardRepository> _logger;

        public DashboardRepository(ApplicationDbContext context, IMapper mapper, ILogger<DashboardRepository> logger)
        {
            this.context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<UnitSummary>> GetUnitSummaryAsync()
        {
            try
            {
                return await context.Units
                    .Include(u => u.Company)
                    .Include(u => u.UnitType)
                    //.Include(u => u.Status)
                    .Select(u => new UnitSummary
                    {
                        Id = u.Id,
                        UnitCode = u.UnitCode,
                        UnitType = u.UnitType.Name ?? "Unknown",
                        Company = u.Company.Name ?? "Unknown",
                        CompanyId = u.CompanyId,
                        SensorCount = u.Sensors.Count()
                        // Add other properties as needed with simpler logic
                    })
                    .AsNoTracking()
                    .ToListAsync();
            }
                // Use separate queries to avoid multiple collection includes
                // This is more efficient and avoids the MultipleCollectionIncludeWarning

                // First, get basic unit information with single navigation properties
                //var units = await context.Units
                //    .Include(u => u.Company)
                //    .Include(u => u.UnitType)
                //    .AsNoTracking()
                //    .ToListAsync();

                //// Get most recent readings for each unit in a separate query
                //var unitIds = units.Select(u => u.Id).ToList();
                //var mostRecentReadings = new List<(int UnitId, DateTime DateReceivedUtc)>();
                
                //try
                //{
                //    // Try to get most recent readings
                //    var readingsQuery = context.MostRecentReadings
                //        .Where(r => unitIds.Contains(r.UnitId))
                //        .AsNoTracking();
                    
                //    mostRecentReadings = await readingsQuery
                //        .Select(r => new { r.UnitId, r.DateReceivedUtc })
                //        .ToListAsync()
                //        .ContinueWith(task => task.Result.Select(r => (r.UnitId, r.DateReceivedUtc)).ToList());
                //}
                //catch (Exception ex)
                //{
                //    _logger.LogWarning(ex, "Error querying MostRecentReadings table. This might be due to schema issues. Skipping most recent readings.");
                //    // Continue without most recent readings
                //}

                //// Get sensor counts for each unit
                //var sensorCounts = await context.Sensors
                //    .Where(s => unitIds.Contains(s.UnitId))
                //    .GroupBy(s => s.UnitId)
                //    .Select(g => new { UnitId = g.Key, Count = g.Count() })
                //    .AsNoTracking()
                //    .ToListAsync();

                //// Get unit statuses for communication info
                //var unitStatuses = new List<(int UnitId, string? Carrier, float? Signal, float? Temperature, bool BatteryAlarm, bool FailedCallout, bool AutoConfig, bool Mip)>();
                //try
                //{
                //    var statusQuery = await context.MostRecentUnitStatuses
                //        .Where(us => unitIds.Contains(us.UnitId))
                //        .AsNoTracking()
                //        .Select(us => new { 
                //            us.UnitId, 
                //            us.Carrier, 
                //            us.Signal, 
                //            us.Temperature, 
                //            us.BatteryAlarm, 
                //            us.FailedCallout, 
                //            us.AutoConfig, 
                //            us.Mip 
                //        })
                //        .ToListAsync();
                    
                //    unitStatuses = statusQuery.Select(us => (us.UnitId, us.Carrier, us.Signal, us.Temperature, us.BatteryAlarm, us.FailedCallout, us.AutoConfig, us.Mip)).ToList();
                //}
                //catch (Exception ex)
                //{
                //    _logger.LogWarning(ex, "Error querying MostRecentUnitStatuses table. Continuing without unit status data.");
                //}

                //// Create summary objects
                //var dashboards = units.Select(unit => 
                //{
                //    var mostRecentReading = mostRecentReadings.FirstOrDefault(r => r.UnitId == unit.Id);
                //    var sensorCount = sensorCounts.FirstOrDefault(sc => sc.UnitId == unit.Id)?.Count ?? 0;
                //    var unitStatus = unitStatuses.FirstOrDefault(us => us.UnitId == unit.Id);

                //    return new UnitSummary
                //    {
                //        Id = unit.Id,
                //        UnitType = unit.UnitType?.Name ?? "Unknown",
                //        UnitCode = unit.UnitCode,
                //        LastComms = mostRecentReading.UnitId != 0 ? mostRecentReading.DateReceivedUtc : (DateTime?)null,
                //        // You can add alarm logic here based on sensor data if needed
                //        // Alarm = sensorCount > 0 && (check alarm conditions),
                //        Carrier = unitStatus.UnitId != 0 ? unitStatus.Carrier ?? "Unknown" : "Unknown",
                //        Signal = unitStatus.UnitId != 0 ? (int?)(unitStatus.Signal ?? 0) : null,
                //        // Add temperature if available in unit status
                //        AmbientTemperature = unitStatus.UnitId != 0 ? unitStatus.Temperature : null,
                //        CompanyID = unit.CompanyID,
                //        Company = unit.Company?.Name ?? "Unknown",
                //        SensorCount = sensorCount,
                //        // Add additional status fields
                //        BatteryAlarm = unitStatus.UnitId != 0 ? unitStatus.BatteryAlarm : false,
                //        FailedCallout = unitStatus.UnitId != 0 ? unitStatus.FailedCallout : false,
                //        AutoConfig = unitStatus.UnitId != 0 ? unitStatus.AutoConfig : false,
                //        Mip = unitStatus.UnitId != 0 ? unitStatus.Mip : false
                //    };
                //}).ToList();

                //return dashboards;
            
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unit summary data");
                throw;
            }
        }
    }
}