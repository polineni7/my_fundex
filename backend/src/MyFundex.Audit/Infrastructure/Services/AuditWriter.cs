using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Audit;

public sealed class AuditWriter(AuditDbContext db) : IAuditWriter
{
    public async Task WriteAsync(
        string module,
        string et,
        string eid,
        string action,
        string? oldv,
        string? newv,
        CancellationToken ct
    )
    {
        db.Add(
            new AuditEvent
            {
                AuditId = MyFundex.BuildingBlocks.Ids.Uuid7.NewGuid(),
                Module = module,
                EntityType = et,
                EntityId = eid,
                Action = action,
                OldValue = oldv,
                NewValue = newv,
                CorrelationId = MyFundex.BuildingBlocks.Ids.Uuid7.NewGuid(),
            }
        );
        await db.SaveChangesAsync(ct);
    }
}
