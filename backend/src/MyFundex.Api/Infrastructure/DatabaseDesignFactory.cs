using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyFundex.BuildingBlocks.Abstractions;

namespace MyFundex.Api.Infrastructure;

public sealed class DatabaseDesignFactory : IDesignTimeDbContextFactory<DevBootstrapDbContext>
{
    public DevBootstrapDbContext CreateDbContext(string[] args)
    {
        var connection =
            Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Database=my_fundex;Username=myfundex;Password=myfundex_dev";
        var options = new DbContextOptionsBuilder<DevBootstrapDbContext>()
            .UseNpgsql(
                connection,
                options =>
                    options.MigrationsHistoryTable("__EFMigrationsHistory", "fundex_integration")
            )
            .Options;
        return new DevBootstrapDbContext(options, new MigrationActor());
    }

    private sealed class MigrationActor : ICurrentActor
    {
        public long ActorId => 0;
    }
}
