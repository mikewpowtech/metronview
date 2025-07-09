using System.Threading.Tasks;

namespace Presentation.AlarmServer.Data.SpiderScope;

public interface ISpiderScopeApi
{
    Task<T> GetAsync<T>(string endpoint);
    Task<T> PostAsync<T>(string endpoint, object data);
}