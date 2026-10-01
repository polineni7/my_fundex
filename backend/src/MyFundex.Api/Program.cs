using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyFundex.Administration;
using MyFundex.Api.Infrastructure;
using MyFundex.Audit;
using MyFundex.Broker;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.Configuration;
using MyFundex.Contracts;
using MyFundex.FundedAccounts;
using MyFundex.Identity;
using MyFundex.Intelligence;
using MyFundex.MarketData;
using MyFundex.MasterData;
using MyFundex.Messaging;
using MyFundex.Notifications;
using MyFundex.Payments;
using MyFundex.Portfolio;
using MyFundex.Risk;
using MyFundex.Subscription;
using MyFundex.Trading;
using MyFundex.Wallet;
using MyFundex.Withdrawals;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
        .AddEnvironmentVariables().AddCommandLine(args);
}
builder.AddPlatformSecurity();
var cs = builder.Configuration.GetConnectionString("Postgres")!;
builder.Services.AddMessaging(builder.Configuration);
if (builder.Configuration.GetValue<bool>("Workers:Enabled", true))
{
    builder.Services.AddHostedService<PaymentFulfilmentWorker>();
    builder.Services.AddHostedService<ChallengeWorker>();
    builder.Services.AddHostedService<FinancialWorker>();
    builder.Services.AddHostedService<ChallengeMailWorker>();
}
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentActor, CurrentActor>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddDbContext<DevBootstrapDbContext>(o =>
    o.UseNpgsql(cs, pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "fundex_integration"))
);
builder
    .Services.AddIdentityModule(cs)
    .AddMasterDataModule(cs)
    .AddConfigurationModule(cs)
    .AddSubscriptionModule(cs)
    .AddFundedAccountsModule(cs)
    .AddRiskModule(cs)
    .AddBrokerModule(cs)
    .AddTradingModule(cs)
    .AddPortfolioModule(cs)
    .AddPaymentsModule(cs)
    .AddWalletModule(cs)
    .AddWithdrawalsModule(cs)
    .AddMarketDataModule(cs)
    .AddIntelligenceModule(cs)
    .AddNotificationsModule(cs)
    .AddAuditModule(cs)
    .AddAdministrationModule();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o =>
    o.AddDefaultPolicy(p =>
        p.WithOrigins(
                builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                    ?? ["http://localhost:5173", "http://localhost:5174"]
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    )
);
var jwtKey = builder.Configuration["Jwt:Key"] ?? "";
if (jwtKey.Length < 32 || (!builder.Environment.IsDevelopment() && jwtKey.StartsWith("DEV-ONLY")))
    throw new InvalidOperationException("Configure a strong Jwt:Key for this environment.");
var key = Encoding.UTF8.GetBytes(jwtKey);
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.Events = new JwtBearerEvents { OnTokenValidated = IdentityEndpoints.ValidateTokenAsync };
        o.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddGoogleSignIn(builder.Configuration);
var app = builder.Build();
if (args.Contains("--seed-defaults", StringComparer.Ordinal))
{
    using var scope = app.Services.CreateScope();
    await DefaultDataSeeder.SeedAsync(
        scope.ServiceProvider.GetRequiredService<DevBootstrapDbContext>(),
        CancellationToken.None
    );
    if (builder.Configuration.GetValue<bool>("Bootstrap:SeedAdmin"))
        await AdministratorBootstrap.SeedAsync(
            app.Services,
            builder.Configuration,
            CancellationToken.None
        );
    app.Logger.LogInformation("Default data seeded successfully.");
    return;
}
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet(
    "/health/ready",
    async (DevBootstrapDbContext db, CancellationToken ct) =>
        await db.Database.CanConnectAsync(ct)
            ? Results.Ok(new { status = "Ready" })
            : Results.Problem("Database unavailable.", statusCode: 503)
);
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", utc = DateTimeOffset.UtcNow }));
app.MapPost(
        "/api/v1/auth/register",
        async (RegisterRequest r, IdentityDbContext db, CancellationToken ct) =>
        {
            var errors = InputValidation.Registration(r.Email, r.Password, r.FirstName, r.LastName);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);
            var email = r.Email.Trim().ToLowerInvariant();
            if (await db.Users.AnyAsync(x => x.Email == email, ct))
                return Results.Conflict(new { message = "Email already registered." });
            var u = new User
            {
                UserId = MyFundex.BuildingBlocks.Ids.Uuid7.NewGuid(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password, 12),
                FirstName = r.FirstName.Trim(),
                LastName = r.LastName.Trim(),
            };
            var traderRole = await db.Roles.SingleAsync(x => x.Code == "TRADER", ct);
            db.Add(u);
            db.Add(new UserRole { User = u, RoleInternalId = traderRole.Id });
            await db.SaveChangesAsync(ct);
            return Results.Created(
                $"/api/v1/users/{u.UserId}",
                new
                {
                    userId = u.UserId,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                }
            );
        }
    )
    .AllowAnonymous()
    .RequireRateLimiting("authentication");
app.MapPost(
        "/api/v1/auth/login",
        async (LoginRequest r, IdentityDbContext db, JwtTokenService jwt, CancellationToken ct) =>
        {
            if (
                string.IsNullOrWhiteSpace(r.Email)
                || string.IsNullOrWhiteSpace(r.Password)
                || Encoding.UTF8.GetByteCount(r.Password) > 72
            )
                return Results.Unauthorized();
            var email = r.Email.Trim().ToLowerInvariant();
            var u = await db.Users.SingleOrDefaultAsync(x => x.Email == email, ct);
            if (
                u == null
                || u.Status != "Active"
                || string.IsNullOrEmpty(u.PasswordHash)
                || !BCrypt.Net.BCrypt.Verify(r.Password, u.PasswordHash)
            )
            {
                db.Events.Add(
                    new IdentityEvent
                    {
                        UserInternalId = u?.Id,
                        EventType = "Login",
                        Method = "Password",
                        Succeeded = false,
                    }
                );
                await db.SaveChangesAsync(ct);
                return Results.Unauthorized();
            }
            db.Events.Add(
                new IdentityEvent
                {
                    UserInternalId = u.Id,
                    EventType = "Login",
                    Method = "Password",
                    Succeeded = true,
                }
            );
            await db.SaveChangesAsync(ct);
            var roles = await (
                from ur in db.UserRoles
                join ro in db.Roles on ur.RoleInternalId equals ro.Id
                where ur.UserInternalId == u.Id
                select ro.Code
            ).ToListAsync(ct);
            var perms = await (
                from ur in db.UserRoles
                join rp in db.RolePermissions on ur.RoleInternalId equals rp.RoleInternalId
                join p in db.Permissions on rp.PermissionInternalId equals p.Id
                where ur.UserInternalId == u.Id
                select p.Code
            )
                .Distinct()
                .ToListAsync(ct);
            return Results.Ok(
                new
                {
                    accessToken = jwt.Create(u, roles, perms),
                    user = new
                    {
                        u.UserId,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        roles,
                        permissions = perms,
                    },
                }
            );
        }
    )
    .AllowAnonymous()
    .RequireRateLimiting("authentication");

app.MapIdentityManagement();
var api = app.MapGroup("/api/v1").RequireAuthorization();
api.MapGet(
    "/plans",
    async (SubscriptionDbContext db, CancellationToken ct) =>
        Results.Ok(
            await db
                .Plans.AsNoTracking()
                .Select(x => new
                {
                    x.PlanId,
                    x.Code,
                    x.Name,
                    x.Description,
                })
                .ToListAsync(ct)
        )
);
api.MapGet(
    "/accounts",
    async (
        AccountsDbContext db,
        TradingDbContext trading,
        ICurrentActor actor,
        CancellationToken ct
    ) =>
    {
        var accounts = await db
            .Accounts.AsNoTracking()
            .Where(x => x.UserInternalId == actor.ActorId)
            .OrderByDescending(x => x.Id)
            .Take(200)
            .ToListAsync(ct);
        var ids = accounts.Select(x => x.AccountId).ToArray();
        var paper = await trading
            .Set<PaperBook>()
            .AsNoTracking()
            .Where(x => ids.Contains(x.AccountId))
            .ToDictionaryAsync(x => x.AccountId, ct);
        var live = await trading
            .Set<LiveBook>()
            .AsNoTracking()
            .Where(x => ids.Contains(x.AccountId))
            .ToDictionaryAsync(x => x.AccountId, ct);
        return Results.Ok(
            accounts.Select(x => new
            {
                x.AccountId,
                x.AccountNumber,
                x.TradingMode,
                x.BrokerProvider,
                status = x.Status != "Active" ? x.Status
                : paper.TryGetValue(x.AccountId, out var paperStatus) ? paperStatus.Status
                : live.TryGetValue(x.AccountId, out var liveStatus) ? liveStatus.Status
                : x.Status,
                x.FundedCapital,
                currentBuyingPower = x.TradingMode == "Evaluation"
                && paper.TryGetValue(x.AccountId, out var p)
                    ? p.Cash - p.ReservedCash
                : x.TradingMode == "Funded" && live.TryGetValue(x.AccountId, out var l)
                    ? l.Cash - l.ReservedCash
                : x.CurrentBuyingPower,
                x.CurrencyCode,
            })
        );
    }
);
api.MapGet(
    "/market/instruments",
    async (string? q, MarketDataDbContext db, CancellationToken ct) =>
    {
        q = (q ?? "").Trim().ToUpperInvariant();
        var data = await db
            .Instruments.AsNoTracking()
            .Where(x =>
                x.IsActive
                && (
                    q == "" || x.TradingSymbol.ToUpper().Contains(q) || x.Name.ToUpper().Contains(q)
                )
            )
            .OrderBy(x => x.TradingSymbol)
            .Take(30)
            .Select(x => new
            {
                x.InstrumentId,
                x.InstrumentToken,
                x.TradingSymbol,
                x.Name,
                x.ExchangeCode,
                x.SecurityType,
            })
            .ToListAsync(ct);
        return Results.Ok(data);
    }
);
api.MapGet(
    "/market/quote",
    async (
        string instrumentToken,
        MyFundex.Contracts.IMarketQuoteProvider quotes,
        CancellationToken ct
    ) =>
    {
        var price = await quotes.GetLtpAsync(instrumentToken, ct);
        return price.HasValue
            ? Results.Ok(new { instrumentToken, lastPrice = price.Value })
            : Results.NotFound(new { message = "Quote unavailable." });
    }
);
api.MapPost(
    "/orders",
    async (
        PlaceOrderCommand c,
        OrderService svc,
        PaperTradingEngine paper,
        IFundedAccountReader accounts,
        CancellationToken ct
    ) =>
    {
        var account = await accounts.GetByPublicIdAsync(c.AccountId, ct);
        var r =
            account?.TradingMode == "Evaluation"
                ? await paper.PlaceAsync(c, ct)
                : await svc.PlaceAsync(c, ct);
        return r.Success ? Results.Ok(r.Value) : Results.BadRequest(new { message = r.Error });
    }
);
api.MapPost(
    "/orders/{orderId:guid}/cancel",
    async (Guid orderId, CancelOrderService service, CancellationToken ct) =>
    {
        var result = await service.CancelAsync(orderId, ct);
        return result.Success
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { message = result.Error });
    }
);
api.MapGet(
    "/orders",
    async (TradingDbContext db, ICurrentActor actor, AccountsDbContext adb, CancellationToken ct) =>
    {
        var ids = await adb
            .Accounts.AsNoTracking()
            .Where(x => x.UserInternalId == actor.ActorId)
            .Select(x => x.Id)
            .ToListAsync(ct);
        return Results.Ok(
            await db
                .Orders.AsNoTracking()
                .Where(x => ids.Contains(x.FundedAccountInternalId))
                .OrderByDescending(x => x.CreatedAt)
                .Take(200)
                .Select(x => new
                {
                    x.OrderId,
                    x.BrokerEnvironment,
                    x.BrokerProvider,
                    x.AccountId,
                    x.Symbol,
                    x.InstrumentToken,
                    x.Side,
                    x.OrderType,
                    x.Quantity,
                    x.FilledQuantity,
                    x.AverageFillPrice,
                    x.Status,
                    x.BrokerOrderId,
                    x.PlacedAt,
                    x.CreatedAt,
                })
                .ToListAsync(ct)
        );
    }
);
api.MapGet(
    "/positions",
    async (
        TradingDbContext db,
        ICurrentActor actor,
        AccountsDbContext accounts,
        CancellationToken ct
    ) =>
    {
        var ids = await accounts
            .Accounts.AsNoTracking()
            .Where(x => x.UserInternalId == actor.ActorId)
            .Select(x => x.AccountId)
            .ToListAsync(ct);
        var paper = await db.Set<PaperPosition>()
            .AsNoTracking()
            .Where(x => ids.Contains(x.AccountId))
            .Select(x => new
            {
                x.PositionId,
                x.AccountId,
                x.Symbol,
                x.InstrumentToken,
                x.Quantity,
                x.AverageCost,
                x.RealizedPnl,
                unrealizedPnl = (x.LastPrice - x.AverageCost) * x.Quantity,
                status = x.Quantity == 0 ? "Closed" : "Open",
                mode = "Paper",
            })
            .ToListAsync(ct);
        var live = await db.Set<LivePosition>()
            .AsNoTracking()
            .Where(x => ids.Contains(x.AccountId))
            .Select(x => new
            {
                x.PositionId,
                x.AccountId,
                x.Symbol,
                x.InstrumentToken,
                x.Quantity,
                x.AverageCost,
                x.RealizedPnl,
                unrealizedPnl = (x.LastPrice - x.AverageCost) * x.Quantity,
                status = x.Quantity == 0 ? "Closed" : "Open",
                mode = "Real",
            })
            .ToListAsync(ct);
        return Results.Ok(paper.Concat(live));
    }
);
api.MapGet(
    "/notifications",
    async (NotificationDbContext db, ICurrentActor actor, CancellationToken ct) =>
        Results.Ok(
            await db
                .Notifications.AsNoTracking()
                .Where(x => x.UserInternalId == actor.ActorId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(100)
                .Select(x => new
                {
                    x.NotificationId,
                    x.Type,
                    x.Title,
                    x.Message,
                    x.IsRead,
                    x.CreatedAt,
                })
                .ToListAsync(ct)
        )
);

var admin = app.MapGroup("/api/v1/admin")
    .RequireAuthorization(p => p.RequireRole("ADMIN", "MANAGER"));
admin.AddEndpointFilter(
    async (context, next) =>
    {
        var user = context.HttpContext.User;
        var path = context.HttpContext.Request.Path.Value ?? "";
        var area =
            path.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(3).FirstOrDefault() ?? "";
        var action = HttpMethods.IsGet(context.HttpContext.Request.Method) ? "read" : "write";
        if (!user.IsInRole("ADMIN") && !user.HasClaim("permission", $"{area}.{action}"))
            return Results.Forbid();
        return await next(context);
    }
);
admin.MapGet(
    "/settings",
    async (ConfigurationDbContext db, CancellationToken ct) =>
        Results.Ok(
            await db
                .Settings.AsNoTracking()
                .OrderBy(x => x.Category)
                .ThenBy(x => x.SettingKey)
                .Select(x => new
                {
                    x.SettingId,
                    x.SettingKey,
                    value = x.IsSensitive
                    || x.SettingKey.ToLower().Contains("token")
                    || x.SettingKey.ToLower().Contains("secret")
                    || x.SettingKey.ToLower().Contains("password")
                        ? "********"
                        : x.SettingValue,
                    x.ValueType,
                    x.Category,
                    x.Environment,
                    x.IsSensitive,
                    x.Description,
                })
                .ToListAsync(ct)
        )
);
admin.MapPut(
    "/settings/{key}",
    async (
        string key,
        SettingUpdateRequest r,
        IRuntimeSettings settings,
        ICurrentActor actor,
        CancellationToken ct
    ) =>
    {
        await settings.UpsertAsync(key, r.Value, r.Environment, r.Category, actor.ActorId, ct);
        return Results.NoContent();
    }
);
admin.MapGet(
    "/accounts",
    async (AccountsDbContext db, TradingDbContext trading, CancellationToken ct) =>
    {
        var accounts = await db
            .Accounts.AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Take(1000)
            .ToListAsync(ct);
        var ids = accounts.Select(x => x.AccountId).ToArray();
        var paper = await trading
            .Set<PaperBook>()
            .AsNoTracking()
            .Where(x => ids.Contains(x.AccountId))
            .ToDictionaryAsync(x => x.AccountId, ct);
        var live = await trading
            .Set<LiveBook>()
            .AsNoTracking()
            .Where(x => ids.Contains(x.AccountId))
            .ToDictionaryAsync(x => x.AccountId, ct);
        return Results.Ok(
            accounts.Select(x => new
            {
                x.AccountId,
                x.UserInternalId,
                x.SubscriptionInternalId,
                x.AccountNumber,
                x.TradingMode,
                x.BrokerProvider,
                status = live.TryGetValue(x.AccountId, out var b) && b.Status != "Active"
                    ? b.Status
                    : x.Status,
                x.FundedCapital,
                currentBuyingPower = paper.TryGetValue(x.AccountId, out var p)
                    ? p.Cash - p.ReservedCash
                : live.TryGetValue(x.AccountId, out var l) ? l.Cash - l.ReservedCash
                : x.CurrentBuyingPower,
                x.Version,
                x.CreatedAt,
            })
        );
    }
);
admin.MapGet(
    "/policies",
    async (RiskDbContext db, CancellationToken ct) =>
        Results.Ok(
            await db
                .PolicySets.AsNoTracking()
                .Select(x => new
                {
                    x.PolicyId,
                    x.Code,
                    x.Name,
                    x.PolicyType,
                })
                .ToListAsync(ct)
        )
);
admin.MapGet(
    "/audit",
    async (AuditDbContext db, CancellationToken ct) =>
        Results.Ok(
            await db
                .Events.AsNoTracking()
                .OrderByDescending(x => x.OccurredAt)
                .Take(500)
                .Select(x => new
                {
                    x.AuditId,
                    x.Module,
                    x.EntityType,
                    x.EntityId,
                    x.Action,
                    x.OccurredAt,
                    x.CorrelationId,
                })
                .ToListAsync(ct)
        )
);

app.MapPost(
        "/api/v1/webhooks/upstox",
        async (HttpRequest req, BrokerDbContext db, CancellationToken ct) =>
        {
            using var sr = new StreamReader(req.Body);
            var raw = await sr.ReadToEndAsync(ct);
            db.Add(
                new BrokerEvent
                {
                    BrokerEventId = MyFundex.BuildingBlocks.Ids.Uuid7.NewGuid(),
                    EventType = "order-update",
                    Payload = raw,
                }
            );
            await db.SaveChangesAsync(ct);
            return Results.Ok();
        }
    )
    .RequireAuthorization(p => p.RequireRole("ADMIN"));

if (builder.Configuration.GetValue<bool>("Database:Migrate"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DevBootstrapDbContext>().Database.MigrateAsync();
}
if (builder.Configuration.GetValue<bool>("Bootstrap:SeedAdmin"))
    await AdministratorBootstrap.SeedAsync(
        app.Services,
        builder.Configuration,
        CancellationToken.None
    );
if (
    app.Environment.IsDevelopment()
    && builder.Configuration.GetValue<bool>("Database:Initialize", true)
)
    await BootstrapAsync(app.Services);
app.MapLifecycleEndpoints();
app.MapPolicyManagement();
app.MapPlanCatalogue();
app.MapGoogleSignIn();
app.MapCommercialEndpoints();
app.MapPlanEndpoints();
app.Run();

static async Task BootstrapAsync(IServiceProvider sp)
{
    using var scope = sp.CreateScope();
    var bootstrap = scope.ServiceProvider.GetRequiredService<DevBootstrapDbContext>();
    await bootstrap.Database.MigrateAsync();
}

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName
);

public sealed record LoginRequest(string Email, string Password);

public sealed record SettingUpdateRequest(
    string Value,
    string Environment = "GLOBAL",
    string Category = "General"
);
