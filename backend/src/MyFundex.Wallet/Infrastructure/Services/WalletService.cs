using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Wallet;

public sealed class WalletService(WalletDbContext db) : IWalletReader, IWalletLedger
{
    public async Task<WalletSnapshot?> GetByAccountAsync(long id, CancellationToken ct) =>
        await db
            .Wallets.AsNoTracking()
            .Where(x => x.FundedAccountInternalId == id)
            .Select(x => new WalletSnapshot(
                x.Id,
                x.WalletId,
                x.FundedAccountInternalId,
                x.CachedAvailableBalance,
                x.CachedWithdrawableBalance
            ))
            .SingleOrDefaultAsync(ct);

    public async Task CreditProfitAsync(
        long id,
        decimal amount,
        string rt,
        string rid,
        CancellationToken ct
    )
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var w = await db.Wallets.SingleAsync(x => x.FundedAccountInternalId == id, ct);
        var t = new LedgerTransaction
        {
            TransactionId = MyFundex.BuildingBlocks.Ids.Uuid7.NewGuid(),
            TransactionType = "ProfitCredit",
            ReferenceType = rt,
            ReferenceId = rid,
            Description = "Realized trader profit",
        };
        db.Add(t);
        await db.SaveChangesAsync(ct);
        db.Add(
            new LedgerEntry
            {
                EntryId = MyFundex.BuildingBlocks.Ids.Uuid7.NewGuid(),
                WalletInternalId = w.Id,
                TransactionInternalId = t.Id,
                Direction = "Credit",
                Amount = amount,
            }
        );
        w.CachedAvailableBalance += amount;
        w.CachedWithdrawableBalance += amount;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
