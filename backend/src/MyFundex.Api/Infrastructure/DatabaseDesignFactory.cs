using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyFundex.BuildingBlocks.Abstractions;

namespace MyFundex.Api.Infrastructure;

public sealed class DatabaseDesignFactory : IDesignTimeDbContextFactory<DevBootstrapDbContext>
{
    public DevBootstrapDbContext CreateDbContext(string[] args)
    {
        var root = Directory.GetCurrentDirectory();
        var project = Path.Combine(root, "backend", "src", "MyFundex.Api");
        if (!Directory.Exists(project)) project = root;
        var configuration = new ConfigurationBuilder().SetBasePath(project)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables().Build();
        var connection = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:Postgres before running migrations.");
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
