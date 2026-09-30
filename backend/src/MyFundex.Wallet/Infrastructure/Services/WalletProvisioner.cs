using Microsoft.EntityFrameworkCore;
using MyFundex.Contracts;

namespace MyFundex.Wallet;

public sealed class WalletProvisioner(WalletDbContext db) : IWalletProvisioner
{
    public async Task EnsureAsync(long accountInternalId, CancellationToken ct)
    {
        if (await db.Wallets.AnyAsync(x => x.FundedAccountInternalId == accountInternalId, ct))
            return;
        db.Add(
            new WalletAccount
            {
                WalletId = Guid.NewGuid(),
                FundedAccountInternalId = accountInternalId,
            }
        );
        await db.SaveChangesAsync(ct);
    }
}
