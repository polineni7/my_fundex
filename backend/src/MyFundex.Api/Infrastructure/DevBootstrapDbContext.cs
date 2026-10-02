using Microsoft.EntityFrameworkCore;
using MyFundex.Audit;
using MyFundex.Broker;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Configuration;
using MyFundex.FundedAccounts;
using MyFundex.Identity;
using MyFundex.Intelligence;
using MyFundex.MarketData;
using MyFundex.MasterData;
using MyFundex.Notifications;
using MyFundex.Payments;
using MyFundex.Portfolio;
using MyFundex.Risk;
using MyFundex.Subscription;
using MyFundex.Trading;
using MyFundex.Wallet;
using MyFundex.Withdrawals;

namespace MyFundex.Api.Infrastructure;

/// <summary>
/// Development-only aggregate model so a fresh local PostgreSQL database can be created in one pass.
/// Production deployments should use reviewed migrations per module/schema.
/// </summary>
public sealed class DevBootstrapDbContext(
    DbContextOptions<DevBootstrapDbContext> o,
    ICurrentActor actor
) : AuditableDbContext(o, actor)
{
    protected override void OnModelCreating(ModelBuilder m)
    {
        Map<User>(m, "Users", "fundex_identity");
        Map<Role>(m, "Roles", "fundex_identity");
        Map<Permission>(m, "Permissions", "fundex_identity");
        Map<UserRole>(m, "UserRoles", "fundex_identity");
        Map<RolePermission>(m, "RolePermissions", "fundex_identity");
        Map<Country>(m, "Countries", "fundex_master");
        Map<Currency>(m, "Currencies", "fundex_master");
        Map<Exchange>(m, "Exchanges", "fundex_master");
        Map<SecurityType>(m, "SecurityTypes", "fundex_master");
        Map<SystemSetting>(m, "Settings", "fundex_configuration");
        Map<Plan>(m, "Plans", "fundex_subscription");
        Map<PlanVersion>(m, "PlanVersions", "fundex_subscription");
        Map<PlanStageDefinition>(m, "Stages", "fundex_subscription");
        Map<UserSubscription>(m, "Subscriptions", "fundex_subscription");
        Map<FundedAccount>(m, "Accounts", "fundex_accounts");
        Map<CapitalAllocation>(m, "CapitalAllocations", "fundex_accounts");
        Map<AccountStatusHistory>(m, "StatusHistory", "fundex_accounts");
        Map<PolicySet>(m, "PolicySets", "fundex_risk");
        Map<PolicyVersion>(m, "PolicyVersions", "fundex_risk");
        Map<PolicyRule>(m, "Rules", "fundex_risk");
        Map<PolicyAssignment>(m, "Assignments", "fundex_risk");
        Map<PolicyViolation>(m, "Violations", "fundex_risk");
        Map<BrokerAccount>(m, "Accounts", "fundex_broker");
        Map<BrokerEvent>(m, "Events", "fundex_broker");
        Map<Order>(m, "Orders", "fundex_trading");
        Map<OrderEvent>(m, "OrderEvents", "fundex_trading");
        Map<Execution>(m, "Executions", "fundex_trading");
        Map<Position>(m, "Positions", "fundex_portfolio");
        Map<PositionLot>(m, "Lots", "fundex_portfolio");
        Map<PaymentTransaction>(m, "Payments", "fundex_payments");
        Map<Invoice>(m, "Invoices", "fundex_payments");
        Map<Receipt>(m, "Receipts", "fundex_payments");
        Map<WalletAccount>(m, "Wallets", "fundex_wallet");
        Map<LedgerTransaction>(m, "Transactions", "fundex_wallet");
        Map<LedgerEntry>(m, "Entries", "fundex_wallet");
        Map<WithdrawalRequest>(m, "Requests", "fundex_withdrawal");
        Map<WithdrawalCalculation>(m, "Calculations", "fundex_withdrawal");
        Map<Instrument>(m, "Instruments", "fundex_market");
        Map<TradingCalendarDay>(m, "TradingCalendar", "fundex_market");
        Map<InstrumentSignal>(m, "Signals", "fundex_intelligence");
        Map<Notification>(m, "Notifications", "fundex_notification");
        Map<AuditEvent>(m, "Events", "fundex_audit");
        MyFundex.Administration.AdministrationDbContext.ConfigureModel(m);
        IdentityDbContext.ConfigureModel(m);
        MasterDataDbContext.ConfigureModel(m);
        ConfigurationDbContext.ConfigureModel(m);
        SubscriptionDbContext.ConfigureModel(m);
        AccountsDbContext.ConfigureModel(m);
        RiskDbContext.ConfigureModel(m);
        BrokerDbContext.ConfigureModel(m);
        TradingDbContext.ConfigureModel(m);
        PortfolioDbContext.ConfigureModel(m);
        PaymentsDbContext.ConfigureModel(m);
        WalletDbContext.ConfigureModel(m);
        WithdrawalDbContext.ConfigureModel(m);
        MarketDataDbContext.ConfigureModel(m);
        IntelligenceDbContext.ConfigureModel(m);
        NotificationDbContext.ConfigureModel(m);
        AuditDbContext.ConfigureModel(m);
    }

    private static void Map<T>(ModelBuilder m, string table, string schema)
        where T : EntityBase
    {
        var b = m.Entity<T>();
        b.ToTable(table, schema);
        ConfigureEntity(b);
    }
}
