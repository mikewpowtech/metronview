using Api;
using Application;
using Application.Identity;
using Domain;
using FluentAssertions;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ApiTest.ReadingEndpoint
{
    public class ReadingEndpointTest(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task GetReadingsBySensorIdTest()
        {
            // First, authenticate to get a token
            var loginResponse = await AuthenticateAsync();
            loginResponse.Should().NotBeNull();

            // Set the authorization header
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);

            // Test the new endpoint with a sensor ID
            var sensorId = 1; // Assuming sensor ID 1 exists in test data
            var response = await _client.GetAsync($"/api/reading/by-sensor/{sensorId}");
            
            response.IsSuccessStatusCode.Should().BeTrue();
            
            var readings = await response.Content.ReadFromJsonAsync<List<Reading>>();
            readings.Should().NotBeNull();
            
            // All readings should belong to the specified sensor
            foreach (var reading in readings!)
            {
                reading.SensorId.Should().Be(sensorId);
            }
        }

        [Fact]
        public async Task GetReadingsBySensorIdNotFoundTest()
        {
            // First, authenticate to get a token
            var loginResponse = await AuthenticateAsync();
            loginResponse.Should().NotBeNull();

            // Set the authorization header
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);

            // Test with a non-existent sensor ID
            var nonExistentSensorId = 99999;
            var response = await _client.GetAsync($"/api/reading/by-sensor/{nonExistentSensorId}");
            
            response.IsSuccessStatusCode.Should().BeTrue();
            
            var readings = await response.Content.ReadFromJsonAsync<List<Reading>>();
            readings.Should().NotBeNull();
            readings!.Should().BeEmpty(); // Should return empty list for non-existent sensor
        }

        private async Task<UserLoginResponse?> AuthenticateAsync()
        {
            var userLoginRequest = new UserLoginRequest
            {
                Email = "UnifiedAppAdmin",
                Password = "UnifiedAppAdmin1!"
            };

            var jsonContent = JsonSerializer.Serialize(userLoginRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/User/Login", content);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var loginResponseWrapper = await response.Content.ReadFromJsonAsync<AppResponse<UserLoginResponse>>();
            return loginResponseWrapper?.Data;
        }
    }
}
