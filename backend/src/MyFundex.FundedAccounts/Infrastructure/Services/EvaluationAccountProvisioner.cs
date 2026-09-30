using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.FundedAccounts;

public sealed class EvaluationAccountProvisioner(
    AccountsDbContext db,
    IWalletProvisioner wallets,
    IPolicyAssignmentService policies
) : IEvaluationAccountProvisioner
{
    public async Task<Guid> ProvisionAsync(EvaluationAccountRequest request, CancellationToken ct)
    {
        if (request.Capital <= 0 || request.ProvisioningId == Guid.Empty || request.UserId <= 0)
            throw new ArgumentException("Invalid evaluation allocation.");
        var account = await db.Accounts.SingleOrDefaultAsync(
            x => x.ProvisioningId == request.ProvisioningId,
            ct
        );
        if (account is null)
        {
            account = new FundedAccount
            {
                AccountId = Guid.NewGuid(),
                ProvisioningId = request.ProvisioningId,
                UserInternalId = request.UserId,
                SubscriptionInternalId = request.SubscriptionInternalId,
                FundedCapital = request.Capital,
                CurrentBuyingPower = request.Capital,
                TradingMode = "Evaluation",
                Status = "Provisioning",
                AccountNumber = "MFX-EV-" + request.ProvisioningId.ToString("N"),
            };
            db.Add(account);
            await db.SaveChangesAsync(ct);
        }
        if (account.UserInternalId != request.UserId || account.FundedCapital != request.Capital)
            throw new InvalidOperationException(
                "Provisioning reference was reused with different parameters."
            );
        // Each receiver is idempotent. A retry resumes after a partial failure without reallocating capital.
        await wallets.EnsureAsync(account.Id, ct);
        await policies.AssignActiveAsync(account.Id, request.PolicySetId, ct);
        if (account.Status == "Provisioning")
        {
            account.Status = "Active";
            account.ActivatedAt = DateTimeOffset.UtcNow;
            db.Add(
                new CapitalAllocation
                {
                    AllocationId = Guid.NewGuid(),
                    FundedAccountInternalId = account.Id,
                    Amount = request.Capital,
                    EffectiveAt = DateTimeOffset.UtcNow,
                    ReferenceType = "Evaluation",
                    ReferenceId = request.ProvisioningId.ToString(),
                }
            );
            await db.SaveChangesAsync(ct);
        }
        return account.AccountId;
    }
}
