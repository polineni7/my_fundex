using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Wallet;

public sealed class WithdrawalFunds(WalletDbContext db) : IWithdrawalFunds
{
    public async Task ReserveAsync(
        Guid withdrawalId,
        long accountId,
        decimal amount,
        CancellationToken ct
    )
    {
        if (withdrawalId == Guid.Empty || amount <= 0 || decimal.Round(amount, 2) != amount)
            throw new ArgumentException("Invalid withdrawal.");
        var wallet = await db.Wallets.SingleAsync(x => x.FundedAccountInternalId == accountId, ct);
        var existing = await db.Set<WithdrawalHold>()
            .SingleOrDefaultAsync(x => x.WithdrawalId == withdrawalId, ct);
        if (existing != null)
        {
            if (existing.WalletInternalId != wallet.Id || existing.Amount != amount)
                throw new ArgumentException("Withdrawal reference mismatch.");
            return;
        }
        if (
            wallet.Status != "Active"
            || wallet.CachedWithdrawableBalance < amount
            || wallet.CachedAvailableBalance < amount
        )
            throw new ArgumentException("Insufficient settled withdrawable funds.");
        wallet.CachedAvailableBalance -= amount;
        wallet.CachedWithdrawableBalance -= amount;
        db.Add(
            new WithdrawalHold
            {
                WithdrawalId = withdrawalId,
                WalletInternalId = wallet.Id,
                Amount = amount,
            }
        );
        await db.SaveChangesAsync(ct);
    }

    public async Task CompleteAsync(Guid withdrawalId, bool paid, CancellationToken ct)
    {
        var hold = await db.Set<WithdrawalHold>()
            .SingleAsync(x => x.WithdrawalId == withdrawalId, ct);
        var target =
            paid ? "Paid"
            : hold.Status is "Paid" or "Reversed" ? "Reversed"
            : "Released";
        if (hold.Status == target)
            return;
        if (hold.Status != "Reserved" && !(hold.Status == "Paid" && !paid))
            throw new ArgumentException("Withdrawal funds already finalized.");
        var wallet = await db.Wallets.SingleAsync(x => x.Id == hold.WalletInternalId, ct);
        if (!paid)
        {
            wallet.CachedAvailableBalance += hold.Amount;
            wallet.CachedWithdrawableBalance += hold.Amount;
        }
        hold.Status = target;
        if (paid || target == "Reversed")
        {
            var transaction = new LedgerTransaction
            {
                TransactionId = Guid.NewGuid(),
                PostingKey = "withdrawal:" + target + ":" + withdrawalId,
                TransactionType = "Withdrawal",
                ReferenceType = "Withdrawal",
                ReferenceId = withdrawalId.ToString(),
            };
            db.Add(transaction);
            db.Add(
                new LedgerEntry
                {
                    EntryId = Guid.NewGuid(),
                    WalletInternalId = wallet.Id,
                    Transaction = transaction,
                    Direction = paid ? "Debit" : "Credit",
                    Amount = hold.Amount,
                }
            );
            db.Add(
                new LedgerEntry
                {
                    EntryId = Guid.NewGuid(),
                    WalletInternalId = wallet.Id,
                    Transaction = transaction,
                    LedgerAccount = "PayoutClearing",
                    Direction = paid ? "Credit" : "Debit",
                    Amount = hold.Amount,
                }
            );
        }
        await db.SaveChangesAsync(ct);
    }
}
