using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Wallet;

public static class WalletModule
{
    public static IServiceCollection AddWalletModule(this IServiceCollection s, string cs)
    {
        s.AddDbContext<WalletDbContext>(o => o.UseNpgsql(cs));
        s.AddScoped<WalletService>();
        s.AddScoped<IWithdrawalFunds, WithdrawalFunds>();
        s.AddScoped<IWalletReader>(sp => sp.GetRequiredService<WalletService>());
        s.AddScoped<IWalletLedger>(sp => sp.GetRequiredService<WalletService>());
        s.AddScoped<IWalletProvisioner, WalletProvisioner>();
        return s;
    }
}
