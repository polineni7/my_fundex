using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Broker;

public sealed class BrokerDbContext(DbContextOptions<BrokerDbContext> o, ICurrentActor a)
    : AuditableDbContext(o, a)
{
    public DbSet<BrokerAccount> Accounts => Set<BrokerAccount>();
    public DbSet<BrokerEvent> Events => Set<BrokerEvent>();

    protected override void OnModelCreating(ModelBuilder m) => ConfigureModel(m);

    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_broker");
        ConfigureEntity(m.Entity<BrokerAccount>());
        ConfigureEntity(m.Entity<BrokerEvent>());
        m.Entity<BrokerAccount>().HasIndex(x => x.BrokerAccountId).IsUnique();
        m.Entity<BrokerEvent>().HasIndex(x => x.BrokerEventId).IsUnique();
    }
}
