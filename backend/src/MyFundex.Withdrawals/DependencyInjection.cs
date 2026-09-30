using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Withdrawals;

public static class WithdrawalsModule
{
    public static IServiceCollection AddWithdrawalsModule(this IServiceCollection s, string cs)
    {
        s.AddDbContext<WithdrawalDbContext>(o => o.UseNpgsql(cs));
        return s;
    }
}
