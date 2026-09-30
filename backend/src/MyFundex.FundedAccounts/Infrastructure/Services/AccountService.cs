using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.FundedAccounts;

public sealed class AccountService(AccountsDbContext db)
    : IFundedAccountReader,
        IFundedAccountCapitalService
{
    public async Task<FundedAccountSnapshot?> GetByPublicIdAsync(Guid id, CancellationToken ct) =>
        await db
            .Accounts.AsNoTracking()
            .Where(x => x.AccountId == id)
            .Select(x => new FundedAccountSnapshot(
                x.Id,
                x.AccountId,
                x.UserInternalId,
                x.FundedCapital,
                x.CurrentBuyingPower,
                x.Status,
                x.Version,
                x.TradingMode,
                x.BrokerCredentialKey,
                x.BrokerProvider
            ))
            .SingleOrDefaultAsync(ct);

    public async Task<bool> TryReserveAsync(
        long id,
        decimal amount,
        long version,
        CancellationToken ct
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        var rows = await db
            .Accounts.Where(x =>
                x.Id == id
                && x.Version == version
                && x.CurrentBuyingPower >= amount
                && x.Status == "Active"
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(x => x.CurrentBuyingPower, x => x.CurrentBuyingPower - amount)
                        .SetProperty(x => x.Version, x => x.Version + 1)
                        .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow),
                ct
            );
        return rows == 1;
    }

    public Task ReleaseAsync(long id, decimal amount, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        return db
            .Accounts.Where(x => x.Id == id)
            .ExecuteUpdateAsync(
                update =>
                    update
                        .SetProperty(x => x.CurrentBuyingPower, x => x.CurrentBuyingPower + amount)
                        .SetProperty(x => x.Version, x => x.Version + 1)
                        .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow),
                ct
            );
    }
}
