using Infrastructure.Identity;
using Infrastructure.DbClasses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
namespace Infrastructure;

public class ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, 
        UserManager<ApplicationUserDb> userManager, RoleManager<IdentityRole> roleManager)
{
    public async Task InitialiseAsync()
    {
        try
        {
            if (context.Database.IsSqlServer())
            {
                await context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }
    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole("Administrator");

        if (roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            var role = await roleManager.CreateAsync(administratorRole);
            if (role != null)
            {
                await roleManager.AddClaimAsync(administratorRole, new Claim("RoleClaim", "HasRoleView"));
                await roleManager.AddClaimAsync(administratorRole, new Claim("RoleClaim", "HasRoleAdd"));
                await roleManager.AddClaimAsync(administratorRole, new Claim("RoleClaim", "HasRoleEdit"));
                await roleManager.AddClaimAsync(administratorRole, new Claim("RoleClaim", "HasRoleDelete"));
            }
        }

        // Default users
        var administrator = new ApplicationUserDb { UserName = "UnifiedAppAdmin", Email = "UnifiedAppAdmin" };

        if (userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await userManager.CreateAsync(administrator, "UnifiedAppAdmin1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
            }
        }

        // Default trigger types
        await SeedTriggerTypesAsync();

        // Default communication modes
        await SeedCommunicationModesAsync();
    }

    private async Task SeedTriggerTypesAsync()
    {
        var triggerTypes = new List<TriggerTypeDb>
        {
            new TriggerTypeDb { Code = "U", Name = "rising past", Order = 1 },
            new TriggerTypeDb { Code = "D", Name = "falling past", Order = 2 },
            new TriggerTypeDb { Code = "A", Name = "above or equal to", Order = 3 },
            new TriggerTypeDb { Code = "B", Name = "below or equal to", Order = 4 },
            new TriggerTypeDb { Code = "C", Name = "rate of change", Order = 5 },
            new TriggerTypeDb { Code = "S", Name = "not reported for (mins)", Order = 6 }
        };

        // Clear existing trigger types and reseed with new data
        if (await context.TriggerTypes.AnyAsync())
        {
            context.TriggerTypes.RemoveRange(context.TriggerTypes);
            await context.SaveChangesAsync();
        }

        // Add new trigger types
        foreach (var triggerType in triggerTypes)
        {
            context.TriggerTypes.Add(triggerType);
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedCommunicationModesAsync()
    {
        // Clear existing data to allow for updates
        var existingCommunicationModes = await context.RecipientModes.ToListAsync();
        if (existingCommunicationModes.Any())
        {
            context.RecipientModes.RemoveRange(existingCommunicationModes);
            await context.SaveChangesAsync();
        }

        var communicationModes = new List<CommunicationModeDb>
        {
            new CommunicationModeDb { Code = "E", Name = "email", Order = 1 },
            new CommunicationModeDb { Code = "S", Name = "SMS", Order = 2 },
            new CommunicationModeDb { Code = "W", Name = "web service call", Order = 3 },
            new CommunicationModeDb { Code = "M", Name = "MQTT message", Order = 4 },
            new CommunicationModeDb { Code = "U", Name = "upload configuration", Order = 5 }
        };

        foreach (var communicationMode in communicationModes)
        {
            if (!await context.RecipientModes.AnyAsync(cm => cm.Code == communicationMode.Code))
            {
                context.RecipientModes.Add(communicationMode);
            }
        }

        await context.SaveChangesAsync();
    }
}