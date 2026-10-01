using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.FundedAccounts;

public sealed class AccountLifecycle(AccountsDbContext db) : IAccountLifecycle
{
    public async Task CloseEvaluationAsync(Guid accountId, string outcome, CancellationToken ct)
    {
        if (outcome is not ("Passed" or "Failed"))
            throw new ArgumentException("Invalid evaluation outcome.");
        var account = await db.Accounts.SingleAsync(x => x.AccountId == accountId, ct);
        if (account.TradingMode != "Evaluation")
            throw new ArgumentException("An evaluation account is required.");
        if (account.Status == outcome)
            return;
        if (account.Status is not ("Active" or "Suspended"))
            throw new ArgumentException("Account is not active.");
        db.StatusHistory.Add(
            new AccountStatusHistory
            {
                StatusHistoryId = Guid.NewGuid(),
                FundedAccountInternalId = account.Id,
                FromStatus = account.Status,
                ToStatus = outcome,
                ReasonCode = "ChallengeResult",
                ChangedAt = DateTimeOffset.UtcNow,
            }
        );
        account.Status = outcome;
        account.ClosedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
