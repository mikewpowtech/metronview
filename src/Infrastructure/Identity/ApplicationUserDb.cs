using Infrastructure.DbClasses;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class ApplicationUserDb : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    // Add other custom properties here

    public string? CompanyId { get; set; }
    public CompanyDb? Company { get; set; }
}
