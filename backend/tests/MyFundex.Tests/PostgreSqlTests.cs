using Microsoft.EntityFrameworkCore;
using MyFundex.Api.Infrastructure;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.FundedAccounts;
using Xunit;

namespace MyFundex.Tests;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MYFUNDEX_TEST_POSTGRES")))
            Skip =
                "Set MYFUNDEX_TEST_POSTGRES to a dedicated PostgreSQL database ending in _tests.";
    }
}

public sealed class PostgreSqlTests
{
    [PostgreSqlFact]
    public async Task MigrationsAndConcurrentCapitalReservationUsePostgreSql()
    {
        var connection = Environment.GetEnvironmentVariable("MYFUNDEX_TEST_POSTGRES")!;
        var settings = new Npgsql.NpgsqlConnectionStringBuilder(connection);
        Assert.EndsWith("_tests", settings.Database);
        var options = new DbContextOptionsBuilder<DevBootstrapDbContext>()
            .UseNpgsql(
                connection,
                pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "fundex_integration")
            )
            .Options;
        await using (var migration = new DevBootstrapDbContext(options, new Actor()))
            await migration.Database.MigrateAsync();
        var accountOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseNpgsql(connection)
            .Options;
        long id;
        await using (var seed = new AccountsDbContext(accountOptions, new Actor()))
        {
            var account = new FundedAccount
            {
                AccountId = Guid.NewGuid(),
                AccountNumber = "TEST-" + Guid.NewGuid().ToString("N"),
                UserInternalId = 1,
                FundedCapital = 1000,
                CurrentBuyingPower = 1000,
            };
            seed.Add(account);
            await seed.SaveChangesAsync();
            id = account.Id;
        }
        await using var first = new AccountsDbContext(accountOptions, new Actor());
        await using var second = new AccountsDbContext(accountOptions, new Actor());
        var results = await Task.WhenAll(
            new AccountService(first).TryReserveAsync(id, 800, 1, default),
            new AccountService(second).TryReserveAsync(id, 800, 1, default)
        );
        Assert.Single(results, success => success);
        var balance = await first
            .Accounts.AsNoTracking()
            .Where(account => account.Id == id)
            .Select(account => account.CurrentBuyingPower)
            .SingleAsync();
        Assert.Equal(200, balance);
    }

    private sealed class Actor : ICurrentActor
    {
        public long ActorId => 0;
    }
}
