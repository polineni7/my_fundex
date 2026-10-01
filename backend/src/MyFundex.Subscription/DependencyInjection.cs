using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Subscription;

public static class SubscriptionModule
{
    public static IServiceCollection AddSubscriptionModule(this IServiceCollection s, string cs)
    {
        s.AddDbContext<SubscriptionDbContext>(o => o.UseNpgsql(cs));
        s.AddScoped<IPaidSubscriptionActivator, PaidSubscriptionActivator>();
        s.AddScoped<PlanPublicationService>();
        s.AddScoped<IEvaluationTermsReader, EvaluationTermsReader>();
        s.AddScoped<ChallengeProgressionService>();
        s.AddScoped<ILiveEntitlementReader, LiveEntitlementReader>();
        s.AddScoped<ILiveRiskTermsReader, LiveRiskTermsReader>();
        s.AddScoped<SubscriptionPurchaseService>();
        s.AddScoped<ITradingEligibility, TradingEligibilityService>();
        return s;
    }
}
