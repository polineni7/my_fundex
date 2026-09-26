using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Identity;
using MyFundex.MasterData;
using MyFundex.Configuration;
using MyFundex.Subscription;
using MyFundex.FundedAccounts;
using MyFundex.Risk;
using MyFundex.Broker;
using MyFundex.Trading;
using MyFundex.Portfolio;
using MyFundex.Payments;
using MyFundex.Wallet;
using MyFundex.Withdrawals;
using MyFundex.MarketData;
using MyFundex.Intelligence;
using MyFundex.Notifications;
using MyFundex.Audit;

namespace MyFundex.Api.Infrastructure;

/// <summary>
/// Development-only aggregate model so a fresh local PostgreSQL database can be created in one pass.
/// Production deployments should use reviewed migrations per module/schema.
/// </summary>
public sealed class DevBootstrapDbContext(DbContextOptions<DevBootstrapDbContext> o, ICurrentActor actor)
    : AuditableDbContext(o, actor)
{
    protected override void OnModelCreating(ModelBuilder m)
    {
        Map<User>(m,"Users","identity"); Map<Role>(m,"Roles","identity"); Map<Permission>(m,"Permissions","identity"); Map<UserRole>(m,"UserRoles","identity"); Map<RolePermission>(m,"RolePermissions","identity");
        Map<Country>(m,"Countries","master"); Map<Currency>(m,"Currencies","master"); Map<Exchange>(m,"Exchanges","master"); Map<SecurityType>(m,"SecurityTypes","master");
        Map<SystemSetting>(m,"Settings","configuration");
        Map<Plan>(m,"Plans","subscription"); Map<PlanVersion>(m,"PlanVersions","subscription"); Map<PlanStageDefinition>(m,"Stages","subscription"); Map<UserSubscription>(m,"Subscriptions","subscription");
        Map<FundedAccount>(m,"Accounts","accounts"); Map<CapitalAllocation>(m,"CapitalAllocations","accounts"); Map<AccountStatusHistory>(m,"StatusHistory","accounts");
        Map<PolicySet>(m,"PolicySets","risk"); Map<PolicyVersion>(m,"PolicyVersions","risk"); Map<PolicyRule>(m,"Rules","risk"); Map<PolicyAssignment>(m,"Assignments","risk"); Map<PolicyViolation>(m,"Violations","risk");
        Map<BrokerAccount>(m,"Accounts","broker"); Map<BrokerEvent>(m,"Events","broker");
        Map<Order>(m,"Orders","trading"); Map<OrderEvent>(m,"OrderEvents","trading"); Map<Execution>(m,"Executions","trading");
        Map<Position>(m,"Positions","portfolio"); Map<PositionLot>(m,"Lots","portfolio");
        Map<PaymentTransaction>(m,"Payments","payments"); Map<Invoice>(m,"Invoices","payments"); Map<Receipt>(m,"Receipts","payments");
        Map<WalletAccount>(m,"Wallets","wallet"); Map<LedgerTransaction>(m,"Transactions","wallet"); Map<LedgerEntry>(m,"Entries","wallet");
        Map<WithdrawalRequest>(m,"Requests","withdrawal"); Map<WithdrawalCalculation>(m,"Calculations","withdrawal");
        Map<Instrument>(m,"Instruments","market"); Map<TradingCalendarDay>(m,"TradingCalendar","market");
        Map<InstrumentSignal>(m,"Signals","intelligence"); Map<Notification>(m,"Notifications","notification"); Map<AuditEvent>(m,"Events","audit");
        IdentityDbContext.ConfigureModel(m); MasterDataDbContext.ConfigureModel(m);
        ConfigurationDbContext.ConfigureModel(m); SubscriptionDbContext.ConfigureModel(m);
        AccountsDbContext.ConfigureModel(m); RiskDbContext.ConfigureModel(m);
        BrokerDbContext.ConfigureModel(m); TradingDbContext.ConfigureModel(m);
        PortfolioDbContext.ConfigureModel(m); PaymentsDbContext.ConfigureModel(m);
        WalletDbContext.ConfigureModel(m); WithdrawalDbContext.ConfigureModel(m);
        MarketDataDbContext.ConfigureModel(m); IntelligenceDbContext.ConfigureModel(m);
        NotificationDbContext.ConfigureModel(m); AuditDbContext.ConfigureModel(m);
    }

    private static void Map<T>(ModelBuilder m, string table, string schema) where T: EntityBase
    {
        var b=m.Entity<T>(); b.ToTable(table,schema); ConfigureEntity(b);
    }
}
