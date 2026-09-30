using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Payments;

public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection s, string cs)
    {
        s.AddHttpClient<RazorpayGateway>();
        s.AddDbContext<PaymentsDbContext>(o => o.UseNpgsql(cs));
        return s;
    }
}
