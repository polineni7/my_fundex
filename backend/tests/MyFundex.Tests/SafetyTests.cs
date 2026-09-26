using Xunit;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MyFundex.Api.Infrastructure;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.Contracts;
using MyFundex.Payments;
using MyFundex.Trading;
using MyFundex.Risk;

namespace MyFundex.Tests;
public sealed class SafetyTests
{
    private static PlaceOrderCommand Order => new(Guid.NewGuid(),"NSE_EQ|TEST","TEST","BUY","LIMIT",1,100,Guid.NewGuid());
    [Fact] public void RegistrationRejectsMissingFieldsAndOversizedBcryptInput()
    {
        Assert.Equal(4,InputValidation.Registration(null,null,null,null).Count);
        Assert.Contains("password",InputValidation.Registration("a@example.com",new string('é',40),"A","B").Keys);
        Assert.Empty(InputValidation.Registration("a@example.com","long-password-123","A","B"));
    }
    [Theory][InlineData(0)][InlineData(-1)][InlineData(1.5)]
    public void EquityQuantityMustBePositiveWholeNumber(decimal quantity)=>Assert.NotNull(OrderValidation.Validate(Order with {Quantity=quantity}));
    [Fact] public void OrderValidationEnforcesPriceSideAndIdempotency()
    {
        Assert.Null(OrderValidation.Validate(Order));
        Assert.NotNull(OrderValidation.Validate(Order with {Price=null}));
        Assert.NotNull(OrderValidation.Validate(Order with {Side="INVALID"}));
        Assert.NotNull(OrderValidation.Validate(Order with {IdempotencyKey=Guid.Empty}));
        Assert.NotNull(OrderValidation.Validate(Order with {OrderType="MARKET"}));
    }
    [Fact] public void SignatureRejectsTamperingAndMalformedHex()
    {
        const string body="order_123|pay_456",secret="test-secret";
        var signature=Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret),Encoding.UTF8.GetBytes(body)));
        Assert.True(PaymentSignature.Verify(body,signature,secret));
        Assert.False(PaymentSignature.Verify(body+"x",signature,secret));
        Assert.False(PaymentSignature.Verify(body,new string('z',64),secret));
        Assert.False(PaymentSignature.Verify(body,signature,""));
    }
    [Fact] public void PaymentsPreservePaiseExactly()
    {
        Assert.Equal(10001,PaymentSignature.ToPaise(100.01m));
        Assert.Throws<ArgumentException>(()=>PaymentSignature.ToPaise(1.001m));
        Assert.Throws<ArgumentException>(()=>PaymentSignature.ToPaise(0));
    }
    [Fact] public async Task AccountOwnershipCheckedBeforeDatabaseOrBrokerAccess()
    {
        var command=Order;
        using var db=new TradingDbContext(new DbContextOptionsBuilder<TradingDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,new Actor());
        var service=new OrderService(db,new Accounts(command.AccountId),null!,null!,null!,null!,new Actor());
        var result=await service.PlaceAsync(command,default);
        Assert.False(result.Success);
        Assert.Equal("Account not found.",result.Error);
    }
    [Fact] public async Task NoAssignedPolicyRejectsTrading()
    {
        using var db=new RiskDbContext(new DbContextOptionsBuilder<RiskDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,new Actor());
        var result=await new RiskGuard(db).EvaluateAsync(new(1,Guid.NewGuid(),"NSE_EQ|TEST","BUY",1,100,100),default);
        Assert.False(result.Allowed);Assert.Equal("POLICY_MISSING",result.Code);
    }
    [Fact] public void BootstrapIncludesModuleIndexesAndCorrectSchemas()
    {
        using var db=new DevBootstrapDbContext(new DbContextOptionsBuilder<DevBootstrapDbContext>().UseNpgsql("Host=localhost;Database=model_only").Options,new Actor());
        var sql=db.Database.GenerateCreateScript();
        Assert.Contains("CREATE UNIQUE INDEX",sql);
        Assert.Contains("\"IsDeleted\" = false",sql);
        Assert.Contains("trading.\"Orders\"",sql);
        Assert.Contains("payments.\"Payments\"",sql);
        Assert.DoesNotContain("is_deleted",sql);
    }
    private sealed class Actor:ICurrentActor { public long ActorId=>10; }
    private sealed class Accounts(Guid id):IFundedAccountReader
    {
        public Task<FundedAccountSnapshot?> GetByPublicIdAsync(Guid accountId,CancellationToken ct)=>Task.FromResult<FundedAccountSnapshot?>(new(1,id,20,1000,1000,"Active",1));
    }
}
