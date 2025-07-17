using Domain;

namespace Application.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        this.dashboardRepository = dashboardRepository;
    }

    public async Task<List<UnitSummary>> GetDashboardAsync()
    {
        return await dashboardRepository.GetUnitSummaryAsync();
    }

    // Add more dashboard-related service methods as needed
}