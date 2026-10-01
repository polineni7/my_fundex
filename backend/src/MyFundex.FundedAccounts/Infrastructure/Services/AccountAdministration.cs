using Microsoft.EntityFrameworkCore;

namespace MyFundex.FundedAccounts;

public sealed class AccountAdministration(AccountsDbContext db)
{
    public async Task SetSuspendedAsync(
        Guid accountId,
        bool suspend,
        long version,
        string reason,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 200)
            throw new ArgumentException("A reason is required.");
        var account = await db.Accounts.SingleAsync(x => x.AccountId == accountId, ct);
        if (account.Version != version)
            throw new DbUpdateConcurrencyException();
        var target = suspend ? "Suspended" : "Active";
        if (account.Status == target)
            return;
        if (suspend && account.Status != "Active" || !suspend && account.Status != "Suspended")
            throw new ArgumentException(
                "Only active accounts can be suspended and suspended accounts resumed."
            );
        db.StatusHistory.Add(
            new AccountStatusHistory
            {
                StatusHistoryId = Guid.NewGuid(),
                FundedAccountInternalId = account.Id,
                FromStatus = account.Status,
                ToStatus = target,
                ReasonCode = "AdministratorReview",
                Reason = reason,
                ChangedAt = DateTimeOffset.UtcNow,
            }
        );
        account.Status = target;
        account.SuspendedAt = suspend ? DateTimeOffset.UtcNow : null;
        await db.SaveChangesAsync(ct);
    }
}
