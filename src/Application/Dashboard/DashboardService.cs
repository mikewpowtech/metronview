using Domain;

namespace Application.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<List<UnitSummary>> GetDashboardAsync()
    {
        return await _dashboardRepository.GetUnitSummaryAsync();
    }

    // Add more dashboard-related service methods as needed
}