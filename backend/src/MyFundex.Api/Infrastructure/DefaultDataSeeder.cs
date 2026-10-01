using Microsoft.EntityFrameworkCore;

namespace MyFundex.Api.Infrastructure;

public static class DefaultDataSeeder
{
    public static async Task SeedAsync(DevBootstrapDbContext db, CancellationToken ct)
    {
        await using var stream =
            typeof(DefaultDataSeeder).Assembly.GetManifestResourceStream("MyFundex.DefaultData.sql")
            ?? throw new InvalidOperationException("Default data resource is missing.");
        using var reader = new StreamReader(stream);
        var sql = await reader.ReadToEndAsync(ct);
        await db.Database.ExecuteSqlRawAsync(sql, ct);
    }
}
