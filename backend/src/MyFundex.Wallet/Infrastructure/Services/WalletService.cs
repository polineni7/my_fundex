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

    public Task CreditProfitAsync(
        long id,
        decimal amount,
        string rt,
        string rid,
        CancellationToken ct
    ) => CreditProfitAllocationAsync(id, amount, 0, 0, rt, rid, ct);

    public async Task CreditProfitAllocationAsync(
        long id,
        decimal amount,
        decimal tax,
        decimal other,
        string rt,
        string rid,
        CancellationToken ct
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        ArgumentOutOfRangeException.ThrowIfNegative(tax);
        ArgumentOutOfRangeException.ThrowIfNegative(other);
        if (
            string.IsNullOrWhiteSpace(rt)
            || string.IsNullOrWhiteSpace(rid)
            || rt.Length > 100
            || rid.Length > 200
        )
            throw new ArgumentException("A bounded settlement reference is required.");
        var postingKey = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(
                    System.Text.Json.JsonSerializer.Serialize(
                        new
                        {
                            id,
                            rt,
                            rid,
                        }
                    )
                )
            )
        );
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var existing = await db
            .Transactions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.PostingKey == postingKey, ct);
        if (existing != null)
        {
            var posted = await db
                .Entries.AsNoTracking()
                .SingleAsync(
                    x =>
                        x.TransactionInternalId == existing.Id
                        && x.LedgerAccount == "TraderPayable",
                    ct
                );
            if (posted.Amount != amount)
                throw new ArgumentException("Settlement reference has a different amount.");
            var deductions = await db
                .Entries.AsNoTracking()
                .Where(x =>
                    x.TransactionInternalId == existing.Id
                    && (x.LedgerAccount == "TaxWithheld" || x.LedgerAccount == "OtherDeductions")
                )
                .ToListAsync(ct);
            if (
                deductions.Where(x => x.LedgerAccount == "TaxWithheld").Sum(x => x.Amount) != tax
                || deductions.Where(x => x.LedgerAccount == "OtherDeductions").Sum(x => x.Amount)
                    != other
            )
                throw new ArgumentException(
                    "Settlement deductions differ from the original posting."
                );
            return;
        }
        var w = await db.Wallets.SingleAsync(x => x.FundedAccountInternalId == id, ct);
        var t = new LedgerTransaction
        {
            TransactionId = MyFundex.BuildingBlocks.Ids.Uuid7.NewGuid(),
            TransactionType = "ProfitCredit",
            PostingKey = postingKey,
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
        db.Add(
            new LedgerEntry
            {
                EntryId = Guid.NewGuid(),
                WalletInternalId = w.Id,
                TransactionInternalId = t.Id,
                LedgerAccount = "PlatformSettlement",
                Direction = "Debit",
                Amount = amount + tax + other,
            }
        );
        foreach (
            var deduction in new[]
            {
                (Account: "TaxWithheld", Amount: tax),
                (Account: "OtherDeductions", Amount: other),
            }
        )
            if (deduction.Amount > 0)
                db.Add(
                    new LedgerEntry
                    {
                        EntryId = Guid.NewGuid(),
                        WalletInternalId = w.Id,
                        TransactionInternalId = t.Id,
                        LedgerAccount = deduction.Account,
                        Direction = "Credit",
                        Amount = deduction.Amount,
                    }
                );
        w.CachedAvailableBalance += amount;
        w.CachedWithdrawableBalance += amount;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
