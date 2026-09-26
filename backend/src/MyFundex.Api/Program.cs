using System.Text;using BCrypt.Net;using Microsoft.AspNetCore.Authentication.JwtBearer;using Microsoft.EntityFrameworkCore;using Microsoft.IdentityModel.Tokens;using MyFundex.Api.Infrastructure;using MyFundex.Administration;using MyFundex.Audit;using MyFundex.Broker;using MyFundex.BuildingBlocks.Abstractions;using MyFundex.Configuration;using MyFundex.FundedAccounts;using MyFundex.Identity;using MyFundex.Intelligence;using MyFundex.MasterData;using MyFundex.MarketData;using MyFundex.Notifications;using MyFundex.Payments;using MyFundex.Portfolio;using MyFundex.Risk;using MyFundex.Subscription;using MyFundex.Trading;using MyFundex.Wallet;using MyFundex.Withdrawals;

var builder=WebApplication.CreateBuilder(args);var cs=builder.Configuration.GetConnectionString("Postgres")!;
builder.Services.AddHttpContextAccessor();builder.Services.AddScoped<ICurrentActor,CurrentActor>();builder.Services.AddSingleton<JwtTokenService>();builder.Services.AddDbContext<DevBootstrapDbContext>(o=>o.UseNpgsql(cs));
builder.Services.AddIdentityModule(cs).AddMasterDataModule(cs).AddConfigurationModule(cs).AddSubscriptionModule(cs).AddFundedAccountsModule(cs).AddRiskModule(cs).AddBrokerModule(cs).AddTradingModule(cs).AddPortfolioModule(cs).AddPaymentsModule(cs).AddWalletModule(cs).AddWithdrawalsModule(cs).AddMarketDataModule(cs).AddIntelligenceModule(cs).AddNotificationsModule(cs).AddAuditModule(cs).AddAdministrationModule();
builder.Services.AddEndpointsApiExplorer();builder.Services.AddSwaggerGen();builder.Services.AddCors(o=>o.AddDefaultPolicy(p=>p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
var key=Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>{o.TokenValidationParameters=new(){ValidateIssuer=true,ValidateAudience=true,ValidateLifetime=true,ValidateIssuerSigningKey=true,ValidIssuer=builder.Configuration["Jwt:Issuer"],ValidAudience=builder.Configuration["Jwt:Audience"],IssuerSigningKey=new SymmetricSecurityKey(key)};});builder.Services.AddAuthorization();
var app=builder.Build();app.UseMiddleware<ExceptionMiddleware>();app.UseCors();app.UseAuthentication();app.UseAuthorization();if(app.Environment.IsDevelopment()){app.UseSwagger();app.UseSwaggerUI();}

app.MapGet("/health",()=>Results.Ok(new{status="Healthy",utc=DateTimeOffset.UtcNow}));
app.MapPost("/api/v1/auth/register",async(RegisterRequest r,IdentityDbContext db,CancellationToken ct)=>{var email=r.Email.Trim().ToLowerInvariant();if(await db.Users.AnyAsync(x=>x.Email==email,ct))return Results.Conflict(new{message="Email already registered."});var u=new User{UserId=MyFundex.BuildingBlocks.Ids.Uuid7.NewGuid(),Email=email,PasswordHash=BCrypt.Net.BCrypt.HashPassword(r.Password),FirstName=r.FirstName.Trim(),LastName=r.LastName.Trim()};db.Add(u);await db.SaveChangesAsync(ct);return Results.Created($"/api/v1/users/{u.UserId}",new{userId=u.UserId,u.Email,u.FirstName,u.LastName});}).AllowAnonymous();
app.MapPost("/api/v1/auth/login",async(LoginRequest r,IdentityDbContext db,JwtTokenService jwt,CancellationToken ct)=>{var email=r.Email.Trim().ToLowerInvariant();var u=await db.Users.SingleOrDefaultAsync(x=>x.Email==email,ct);if(u==null||!BCrypt.Net.BCrypt.Verify(r.Password,u.PasswordHash))return Results.Unauthorized();var roles=await(from ur in db.UserRoles join ro in db.Roles on ur.RoleInternalId equals ro.Id where ur.UserInternalId==u.Id select ro.Code).ToListAsync(ct);var perms=await(from ur in db.UserRoles join rp in db.RolePermissions on ur.RoleInternalId equals rp.RoleInternalId join p in db.Permissions on rp.PermissionInternalId equals p.Id where ur.UserInternalId==u.Id select p.Code).Distinct().ToListAsync(ct);return Results.Ok(new{accessToken=jwt.Create(u,roles,perms),user=new{u.UserId,u.Email,u.FirstName,u.LastName,roles,permissions=perms}});}).AllowAnonymous();

var api=app.MapGroup("/api/v1").RequireAuthorization();
api.MapGet("/plans",async(SubscriptionDbContext db,CancellationToken ct)=>Results.Ok(await db.Plans.AsNoTracking().Select(x=>new{x.PlanId,x.Code,x.Name,x.Description}).ToListAsync(ct)));
api.MapGet("/accounts",async(AccountsDbContext db,ICurrentActor actor,CancellationToken ct)=>Results.Ok(await db.Accounts.AsNoTracking().Where(x=>x.UserInternalId==actor.ActorId).Select(x=>new{x.AccountId,x.AccountNumber,x.Status,x.FundedCapital,x.CurrentBuyingPower,x.CurrencyCode}).ToListAsync(ct)));
api.MapGet("/market/instruments",async(string? q,MarketDataDbContext db,CancellationToken ct)=>{q=(q??"").Trim().ToUpperInvariant();var data=await db.Instruments.AsNoTracking().Where(x=>x.IsActive&&(q==""||x.TradingSymbol.ToUpper().Contains(q)||x.Name.ToUpper().Contains(q))).OrderBy(x=>x.TradingSymbol).Take(30).Select(x=>new{x.InstrumentId,x.InstrumentToken,x.TradingSymbol,x.Name,x.ExchangeCode,x.SecurityType}).ToListAsync(ct);return Results.Ok(data);});
api.MapGet("/market/quote",async(string instrumentToken,MyFundex.Contracts.IMarketQuoteProvider quotes,CancellationToken ct)=>{var price=await quotes.GetLtpAsync(instrumentToken,ct);return price.HasValue?Results.Ok(new{instrumentToken,lastPrice=price.Value}):Results.NotFound(new{message="Quote unavailable."});});
api.MapPost("/orders",async(PlaceOrderCommand c,OrderService svc,CancellationToken ct)=>{var r=await svc.PlaceAsync(c,ct);return r.Success?Results.Ok(r.Value):Results.BadRequest(new{message=r.Error});});
api.MapGet("/orders",async(TradingDbContext db,ICurrentActor actor,AccountsDbContext adb,CancellationToken ct)=>{var ids=await adb.Accounts.AsNoTracking().Where(x=>x.UserInternalId==actor.ActorId).Select(x=>x.Id).ToListAsync(ct);return Results.Ok(await db.Orders.AsNoTracking().Where(x=>ids.Contains(x.FundedAccountInternalId)).OrderByDescending(x=>x.CreatedAt).Take(200).Select(x=>new{x.OrderId,x.AccountId,x.Symbol,x.InstrumentToken,x.Side,x.OrderType,x.Quantity,x.Status,x.BrokerOrderId,x.PlacedAt,x.CreatedAt}).ToListAsync(ct));});
api.MapGet("/positions",async(PortfolioDbContext db,ICurrentActor actor,AccountsDbContext adb,CancellationToken ct)=>{var ids=await adb.Accounts.AsNoTracking().Where(x=>x.UserInternalId==actor.ActorId).Select(x=>x.Id).ToListAsync(ct);return Results.Ok(await db.Positions.AsNoTracking().Where(x=>ids.Contains(x.FundedAccountInternalId)).Select(x=>new{x.PositionId,x.Symbol,x.InstrumentToken,x.Quantity,x.AverageCost,x.RealizedPnl,x.UnrealizedPnl,x.Status,x.OpenedAt,x.ClosedAt}).ToListAsync(ct));});
api.MapGet("/notifications",async(NotificationDbContext db,ICurrentActor actor,CancellationToken ct)=>Results.Ok(await db.Notifications.AsNoTracking().Where(x=>x.UserInternalId==actor.ActorId).OrderByDescending(x=>x.CreatedAt).Take(100).Select(x=>new{x.NotificationId,x.Type,x.Title,x.Message,x.IsRead,x.CreatedAt}).ToListAsync(ct)));

var admin=app.MapGroup("/api/v1/admin").RequireAuthorization(p=>p.RequireRole("ADMIN","MANAGER"));
admin.MapGet("/settings",async(ConfigurationDbContext db,CancellationToken ct)=>Results.Ok(await db.Settings.AsNoTracking().OrderBy(x=>x.Category).ThenBy(x=>x.SettingKey).Select(x=>new{x.SettingId,x.SettingKey,value=x.IsSensitive?"********":x.SettingValue,x.ValueType,x.Category,x.Environment,x.IsSensitive,x.Description}).ToListAsync(ct)));
admin.MapPut("/settings/{key}",async(string key,SettingUpdateRequest r,IRuntimeSettings settings,ICurrentActor actor,CancellationToken ct)=>{await settings.UpsertAsync(key,r.Value,r.Environment,r.Category,actor.ActorId,ct);return Results.NoContent();});
admin.MapGet("/accounts",async(AccountsDbContext db,CancellationToken ct)=>Results.Ok(await db.Accounts.AsNoTracking().OrderByDescending(x=>x.CreatedAt).Take(1000).Select(x=>new{x.AccountId,x.UserInternalId,x.AccountNumber,x.Status,x.FundedCapital,x.CurrentBuyingPower,x.Version,x.CreatedAt}).ToListAsync(ct)));
admin.MapGet("/policies",async(RiskDbContext db,CancellationToken ct)=>Results.Ok(await db.PolicySets.AsNoTracking().Select(x=>new{x.PolicyId,x.Code,x.Name,x.PolicyType}).ToListAsync(ct)));
admin.MapGet("/audit",async(AuditDbContext db,CancellationToken ct)=>Results.Ok(await db.Events.AsNoTracking().OrderByDescending(x=>x.OccurredAt).Take(500).Select(x=>new{x.AuditId,x.Module,x.EntityType,x.EntityId,x.Action,x.OccurredAt,x.CorrelationId}).ToListAsync(ct)));

app.MapPost("/api/v1/webhooks/upstox",async(HttpRequest req,BrokerDbContext db,CancellationToken ct)=>{using var sr=new StreamReader(req.Body);var raw=await sr.ReadToEndAsync(ct);db.Add(new BrokerEvent{BrokerEventId=MyFundex.BuildingBlocks.Ids.Uuid7.NewGuid(),EventType="order-update",Payload=raw});await db.SaveChangesAsync(ct);return Results.Ok();}).AllowAnonymous();

await BootstrapAsync(app.Services);
app.Run();

static async Task BootstrapAsync(IServiceProvider sp)
{
    using var scope=sp.CreateScope();
    var bootstrap=scope.ServiceProvider.GetRequiredService<DevBootstrapDbContext>();
    await bootstrap.Database.EnsureCreatedAsync();

    var db=scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    if(!await db.Users.AnyAsync())
    {
        var admin=new User{UserId=MyFundex.BuildingBlocks.Ids.Uuid7.NewGuid(),Email="admin@myfundex.local",PasswordHash=BCrypt.Net.BCrypt.HashPassword("ChangeMe!123"),FirstName="System",LastName="Admin"};
        var role=new Role{RoleId=MyFundex.BuildingBlocks.Ids.Uuid7.NewGuid(),Code="ADMIN",Name="Administrator"};
        db.AddRange(admin,role); await db.SaveChangesAsync();
        db.Add(new UserRole{UserInternalId=admin.Id,RoleInternalId=role.Id}); await db.SaveChangesAsync();
    }
}

public sealed record RegisterRequest(string Email,string Password,string FirstName,string LastName);
public sealed record LoginRequest(string Email,string Password);
public sealed record SettingUpdateRequest(string Value,string Environment="GLOBAL",string Category="General");
