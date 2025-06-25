using Domain;

namespace Application.Dashboard;

public interface IDashboardService
{
    Task<List<UnitSummary>> GetDashboardAsync();
    // Add more dashboard-related service methods as needed
}