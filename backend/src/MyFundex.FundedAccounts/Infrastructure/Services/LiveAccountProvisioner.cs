using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.FundedAccounts;

public sealed class LiveAccountProvisioner(
    AccountsDbContext db,
    ILiveEntitlementReader entitlements,
    IBrokerAccountVerifier verifier,
    IWalletProvisioner wallets,
    IPolicyAssignmentService policies
) : ILiveAccountProvisioner
{
    public async Task<Guid> ProvisionAsync(
        Guid subscriptionId,
        string provider,
        string credentialKey,
        string brokerUserId,
        CancellationToken ct
    )
    {
        var entitlement =
            await entitlements.GetAsync(subscriptionId, ct)
            ?? throw new ArgumentException(
                "All paid evaluation stages must be completed before live allocation."
            );
        if (!await verifier.VerifyAsync(provider, credentialKey, brokerUserId, ct))
            throw new ArgumentException("Broker account could not be verified.");
        var account = await db.Accounts.SingleOrDefaultAsync(
            x => x.ProvisioningId == subscriptionId,
            ct
        );
        if (account == null)
        {
            account = new FundedAccount
            {
                AccountId = Guid.NewGuid(),
                ProvisioningId = subscriptionId,
                SubscriptionInternalId = entitlement.SubscriptionInternalId,
                UserInternalId = entitlement.UserId,
                AccountNumber = "MFX-LIVE-" + subscriptionId.ToString("N"),
                TradingMode = "Funded",
                Status = "Provisioning",
                FundedCapital = entitlement.Capital,
                CurrentBuyingPower = entitlement.Capital,
                BrokerProvider = provider,
                BrokerCredentialKey = credentialKey,
                BrokerUserId = brokerUserId,
            };
            db.Add(account);
            await db.SaveChangesAsync(ct);
        }
        if (
            account.BrokerProvider != provider
            || account.BrokerCredentialKey != credentialKey
            || account.BrokerUserId != brokerUserId
        )
            throw new ArgumentException(
                "This allocation is already linked to a different broker account."
            );
        await wallets.EnsureAsync(account.Id, ct);
        await policies.AssignActiveAsync(account.Id, entitlement.PolicySetId, ct);
        if (account.Status == "Provisioning")
        {
            // Allocation is recorded, but live order acceptance additionally requires the deployment gate.
            account.Status = "Active";
            account.ActivatedAt = DateTimeOffset.UtcNow;
            db.CapitalAllocations.Add(
                new CapitalAllocation
                {
                    AllocationId = Guid.NewGuid(),
                    FundedAccountInternalId = account.Id,
                    Amount = entitlement.Capital,
                    EffectiveAt = DateTimeOffset.UtcNow,
                    ReferenceType = "Graduation",
                    ReferenceId = subscriptionId.ToString(),
                }
            );
            db.StatusHistory.Add(
                new AccountStatusHistory
                {
                    StatusHistoryId = Guid.NewGuid(),
                    FundedAccountInternalId = account.Id,
                    FromStatus = "Provisioning",
                    ToStatus = "Active",
                    ReasonCode = "VerifiedGraduation",
                    ChangedAt = DateTimeOffset.UtcNow,
                }
            );
            await db.SaveChangesAsync(ct);
        }
        return account.AccountId;
    }
}
