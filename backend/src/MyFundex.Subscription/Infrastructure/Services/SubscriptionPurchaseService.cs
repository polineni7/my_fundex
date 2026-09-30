using Microsoft.EntityFrameworkCore;

namespace MyFundex.Subscription;

public sealed class SubscriptionPurchaseService(SubscriptionDbContext db)
{
    public async Task<UserSubscription> CreateAsync(
        long userId,
        Guid versionId,
        Guid requestId,
        Guid? previousSubscriptionId,
        CancellationToken ct
    )
    {
        if (userId <= 0 || requestId == Guid.Empty)
            throw new ArgumentException("A purchase reference is required.");
        var existing = await db.Subscriptions.SingleOrDefaultAsync(
            x => x.PurchaseRequestId == requestId,
            ct
        );
        if (existing != null)
        {
            var existingVersion = await db
                .PlanVersions.AsNoTracking()
                .SingleAsync(x => x.Id == existing.PlanVersionInternalId, ct);
            if (
                existing.UserInternalId != userId
                || existingVersion.PlanVersionId != versionId
                || existing.PreviousSubscriptionId != previousSubscriptionId
            )
                throw new ArgumentException("Purchase reference was used for another request.");
            return existing;
        }
        var now = DateTimeOffset.UtcNow;
        var version =
            await db
                .PlanVersions.AsNoTracking()
                .SingleOrDefaultAsync(
                    x =>
                        x.PlanVersionId == versionId
                        && x.Status == "Active"
                        && x.EffectiveFrom <= now
                        && (x.EffectiveTo == null || x.EffectiveTo > now),
                    ct
                ) ?? throw new ArgumentException("Select an active plan version.");
        if (previousSubscriptionId.HasValue)
        {
            var previous = await db
                .Subscriptions.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.SubscriptionId == previousSubscriptionId && x.UserInternalId == userId,
                    ct
                );
            if (
                previous == null
                || previous.Status != "Failed"
                || previous.PlanInternalId != version.PlanInternalId
            )
                throw new ArgumentException(
                    "Only a failed evaluation can be purchased again for this plan."
                );
        }
        var subscription = new UserSubscription
        {
            SubscriptionId = Guid.NewGuid(),
            PurchaseRequestId = requestId,
            PreviousSubscriptionId = previousSubscriptionId,
            UserInternalId = userId,
            PlanInternalId = version.PlanInternalId,
            PlanVersionInternalId = version.Id,
            SubscribedAt = now,
        };
        db.Subscriptions.Add(subscription);
        await db.SaveChangesAsync(ct);
        return subscription;
    }
}
