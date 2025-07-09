using System;
using Microsoft.EntityFrameworkCore;

namespace Presentation.AlarmServer.Data;

public class AlarmDbContext : DbContext
{
    public AlarmDbContext(DbContextOptions<AlarmDbContext> options) : base(options) { }
    // Define your DbSet properties here, e.g.:
    // public DbSet<Alarm> Alarms { get; set; }

}
