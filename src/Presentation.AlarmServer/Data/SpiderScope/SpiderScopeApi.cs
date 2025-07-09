using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Presentation.AlarmServer.Data.SpiderScope;

public class SpiderScopeApi : ISpiderScopeApi
{
    private readonly HttpClient httpClient;
    private readonly SpiderScopeApiOptions options;
    private readonly ILogger logger;


    public SpiderScopeApi(HttpClient httpClient, IOptions<SpiderScopeApiOptions> options, ILoggerFactory loggerFactory)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
        logger = loggerFactory.CreateLogger<SpiderScopeApi>();

        this.httpClient.BaseAddress = new Uri(this.options.HttpClientBaseAddress);
        var authenticationString = $"{this.options.Username}:{this.options.Password}";
        var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
        this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        //MW I'm leaving the endpoint string here, it make become used for different endpoints in the future
        var response = await httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<T> PostAsync<T>(string endpoint, object data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    // Add other methods as needed (e.g., PutAsync, DeleteAsync)
}
