using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.FundedAccounts;

public static class FundedAccountsModule
{
    public static IServiceCollection AddFundedAccountsModule(this IServiceCollection s, string cs)
    {
        s.AddDbContext<AccountsDbContext>(o => o.UseNpgsql(cs));
        s.AddScoped<AccountService>();
        s.AddScoped<AccountAdministration>();
        s.AddScoped<IAccountLifecycle, AccountLifecycle>();
        s.AddScoped<ILiveAccountProvisioner, LiveAccountProvisioner>();
        s.AddScoped<IFundedAccountReader>(sp => sp.GetRequiredService<AccountService>());
        s.AddScoped<IFundedAccountCapitalService>(sp => sp.GetRequiredService<AccountService>());
        s.AddScoped<IEvaluationAccountProvisioner, EvaluationAccountProvisioner>();
        return s;
    }
}
