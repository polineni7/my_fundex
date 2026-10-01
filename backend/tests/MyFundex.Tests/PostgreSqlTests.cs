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
        var planOptions = new DbContextOptionsBuilder<MyFundex.Subscription.SubscriptionDbContext>()
            .UseNpgsql(connection).Options;
        await using (var plans = new MyFundex.Subscription.SubscriptionDbContext(planOptions, new Actor()))
        {
            await using var planTransaction = await plans.Database.BeginTransactionAsync();
            var omitted = new MyFundex.Subscription.PlanStageDefinition
            {
                StageId = Guid.NewGuid(),
                MinimumTradingDays = null,
                TradingPeriod = null,
                MaximumLeverage = null
            };
            var explicitZero = new MyFundex.Subscription.PlanStageDefinition
            {
                StageId = Guid.NewGuid(),
                MinimumTradingDays = 0,
                TradingPeriod = 0,
                MaximumLeverage = 100
            };
            plans.AddRange(omitted, explicitZero);
            await plans.SaveChangesAsync();
            plans.ChangeTracker.Clear();
            var persistedNull = await plans.Stages.SingleAsync(s => s.StageId == omitted.StageId);
            var persistedZero = await plans.Stages.SingleAsync(s => s.StageId == explicitZero.StageId);
            Assert.Null(persistedNull.MinimumTradingDays);
            Assert.Null(persistedNull.TradingPeriod);
            Assert.Null(persistedNull.MaximumLeverage);
            Assert.Equal(0, persistedZero.MinimumTradingDays);
            Assert.Equal(0, persistedZero.TradingPeriod);
            Assert.Equal(100m, persistedZero.MaximumLeverage);
            await planTransaction.RollbackAsync();
        }
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

    [PostgreSqlFact]
    public async Task IdentityAccessConstraintsAuditAndTokenRevocationUsePostgreSql()
    {
        var connection = Environment.GetEnvironmentVariable("MYFUNDEX_TEST_POSTGRES")!;
        Assert.EndsWith("_tests", new Npgsql.NpgsqlConnectionStringBuilder(connection).Database);
        await using (
            var migrations = new DevBootstrapDbContext(
                new DbContextOptionsBuilder<DevBootstrapDbContext>()
                    .UseNpgsql(
                        connection,
                        pg =>
                            pg.MigrationsHistoryTable("__EFMigrationsHistory", "fundex_integration")
                    )
                    .Options,
                new Actor()
            )
        )
            await migrations.Database.MigrateAsync();
        var options = new DbContextOptionsBuilder<MyFundex.Identity.IdentityDbContext>()
            .UseNpgsql(connection)
            .Options;
        await using var db = new MyFundex.Identity.IdentityDbContext(
            options,
            new Actor(),
            new Microsoft.AspNetCore.DataProtection.EphemeralDataProtectionProvider()
        );
        var user = new MyFundex.Identity.User
        {
            UserId = Guid.NewGuid(),
            Email = Guid.NewGuid().ToString("N") + "@example.invalid",
            FirstName = "Identity",
            LastName = "Test",
        };
        var role = await db.Roles.SingleAsync(x => x.Code == "TRADER");
        db.Add(new MyFundex.Identity.UserRole { User = user, RoleInternalId = role.Id });
        await db.SaveChangesAsync();
        var version = user.SecurityVersion;
        await new MyFundex.Identity.IdentityAdministration(db, new Actor()).UpdateAccessAsync(
            user.UserId,
            user.Version,
            "Suspended",
            ["TRADER"],
            "Integration test",
            default
        );
        Assert.Equal(version + 1, user.SecurityVersion);
        Assert.Equal("Suspended", user.Status);
        Assert.True(
            await db.Events.AnyAsync(x =>
                x.UserInternalId == user.Id && x.EventType == "AccessChanged"
            )
        );
        db.Add(
            new MyFundex.Identity.UserRole { UserInternalId = user.Id, RoleInternalId = role.Id }
        );
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        await using var sql = new Npgsql.NpgsqlConnection(connection);
        await sql.OpenAsync();
        await using var command = new Npgsql.NpgsqlCommand(
            "SELECT count(*) FROM information_schema.schemata WHERE schema_name LIKE 'myfund_%'",
            sql
        );
        Assert.Equal(0L, await command.ExecuteScalarAsync());
        command.CommandText =
            "SELECT count(*) FROM information_schema.tables WHERE table_schema IN ('fundex_prod_trading','fundex_prod_wallet','fundex_sandbox_trading')";
        Assert.Equal(6L, await command.ExecuteScalarAsync());
    }

    [PostgreSqlFact]
    public async Task DefaultDataCanBeSeededTwiceWithoutDuplicatingOrPublishingPlans()
    {
        var connection = Environment.GetEnvironmentVariable("MYFUNDEX_TEST_POSTGRES")!;
        Assert.EndsWith("_tests", new Npgsql.NpgsqlConnectionStringBuilder(connection).Database);
        await using var db = new DevBootstrapDbContext(
            new DbContextOptionsBuilder<DevBootstrapDbContext>()
                .UseNpgsql(
                    connection,
                    pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "fundex_integration")
                )
                .Options,
            new Actor()
        );
        await db.Database.MigrateAsync();
        await DefaultDataSeeder.SeedAsync(db, default);
        var seedVersions = db.Set<MyFundex.Subscription.PlanVersion>().Where(v =>
            v.PlanVersionId == Guid.Parse("8b97c7e7-dbd6-5c39-ab82-a4c0f2ab3318") ||
            v.PlanVersionId == Guid.Parse("f6779897-6557-5179-8c1d-8461806bd3ca"));
        var seedStages = db.Set<MyFundex.Subscription.PlanStageDefinition>()
            .Where(s => seedVersions.Any(v => v.Id == s.PlanVersionInternalId));
        var memberships = await db.Set<MyFundex.Identity.RolePermission>().CountAsync();
        var stages = await seedStages.CountAsync();
        await DefaultDataSeeder.SeedAsync(db, default);
        Assert.Equal(memberships, await db.Set<MyFundex.Identity.RolePermission>().CountAsync());
        Assert.Equal(
            stages,
            await seedStages.CountAsync()
        );
        Assert.Equal(5, stages);
        Assert.True(memberships > 0);
        Assert.False(
            await seedStages.AnyAsync(x => x.PolicySetId == Guid.Empty)
        );
        Assert.Equal(
            2,
            await seedVersions.CountAsync(x => x.Status == "Draft")
        );
    }

    private sealed class Actor : ICurrentActor
    {
        public long ActorId => 0;
    }
}
