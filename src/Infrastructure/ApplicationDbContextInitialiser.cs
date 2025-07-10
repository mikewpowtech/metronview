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
            new TriggerTypeDb { Code = Domain.Enums.TriggerTypeCode.Rising, Name = "rising past", Order = 1 },
            new TriggerTypeDb { Code = Domain.Enums.TriggerTypeCode.Falling, Name = "falling past", Order = 2 },
            new TriggerTypeDb { Code = Domain.Enums.TriggerTypeCode.Above, Name = "above or equal to", Order = 3 },
            new TriggerTypeDb { Code = Domain.Enums.TriggerTypeCode.Below, Name = "below or equal to", Order = 4 },
            new TriggerTypeDb { Code = Domain.Enums.TriggerTypeCode.RateOfChange, Name = "rate of change", Order = 5 },
            new TriggerTypeDb { Code = Domain.Enums.TriggerTypeCode.NotReportedForPeriod, Name = "not reported for (mins)", Order = 6 }
        };

        // Update existing trigger types or add new ones, don't remove
        foreach (var triggerType in triggerTypes)
        {
            var existing = await context.TriggerTypes.FirstOrDefaultAsync(tt => tt.Code == triggerType.Code);
            if (existing != null)
            {
                // Update existing trigger type
                existing.Name = triggerType.Name;
                existing.Order = triggerType.Order;
                context.TriggerTypes.Update(existing);
            }
            else
            {
                // Add new trigger type
                context.TriggerTypes.Add(triggerType);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedCommunicationModesAsync()
    {
        var communicationModes = new List<CommunicationModeDb>
        {
            new CommunicationModeDb { Code = "E", Name = "email", Order = 1 },
            new CommunicationModeDb { Code = "S", Name = "SMS", Order = 2 },
            new CommunicationModeDb { Code = "W", Name = "web service call", Order = 3 },
            new CommunicationModeDb { Code = "M", Name = "MQTT message", Order = 4 },
            new CommunicationModeDb { Code = "U", Name = "upload configuration", Order = 5 }
        };

        // Update existing communication modes or add new ones, don't remove
        foreach (var communicationMode in communicationModes)
        {
            var existing = await context.RecipientModes.FirstOrDefaultAsync(cm => cm.Code == communicationMode.Code);
            if (existing != null)
            {
                // Update existing communication mode
                existing.Name = communicationMode.Name;
                existing.Order = communicationMode.Order;
                context.RecipientModes.Update(existing);
            }
            else
            {
                // Add new communication mode
                context.RecipientModes.Add(communicationMode);
            }
        }

        await context.SaveChangesAsync();
    }
}