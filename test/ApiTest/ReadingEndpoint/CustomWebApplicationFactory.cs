using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTest.ReadingEndpoint
{
    public class CustomWebApplicationFactory<TProgram>
        : WebApplicationFactory<TProgram> where TProgram : class
    {

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                var context = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(ApplicationDbContext));
                if (context != null)
                {
                    services.Remove(context);
                    var options = services.Where(r => r.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>)
                      || r.ServiceType.IsGenericType && r.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>)).ToArray();
                    foreach (var option in options)
                    {
                        services.Remove(option);
                    }
                }

                // Add a new registration for ApplicationDbContext with an in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    // Provide a unique name for your in-memory database
                    options.UseInMemoryDatabase("InMemoryDatabaseName");
                });
            });
        }
    }
}
