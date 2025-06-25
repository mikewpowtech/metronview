using Domain;

namespace Application.Dashboard;

public interface IDashboardRepository
{
    Task<List<UnitSummary>> GetUnitSummaryAsync();
}