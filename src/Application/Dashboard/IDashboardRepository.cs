namespace Application.Dashboard;

public interface IDashboardRepository
{
    Task<List<Domain.Dashboard>> GetDashboardUnitsAsync();
}