using Microsoft.EntityFrameworkCore;
using SentinelAgent.Domain.Entities;

namespace SentinelAgent.Infrastructure.Persistence;

public class IncidentDbContext(DbContextOptions<IncidentDbContext> options) : DbContext(options)
{
    public DbSet<FailureRecord> Failures => Set<FailureRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IncidentDbContext).Assembly);
    }
}