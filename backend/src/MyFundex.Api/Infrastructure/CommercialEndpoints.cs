using Microsoft.EntityFrameworkCore;
using MyFundex.Payments;
using MyFundex.Subscription;
using MyFundex.BuildingBlocks.Abstractions;
using System.Text.Json;

namespace MyFundex.Api.Infrastructure;

public static class CommercialEndpoints
{
    public static void MapCommercialEndpoints(this WebApplication app)
    {
        var api=app.MapGroup("/api/v1").RequireAuthorization();
        api.MapGet("/subscriptions",async(SubscriptionDbContext db,ICurrentActor actor,CancellationToken ct)=>
            Results.Ok(await (from subscription in db.Subscriptions.AsNoTracking()
                join plan in db.Plans.AsNoTracking() on subscription.PlanInternalId equals plan.Id
                join version in db.PlanVersions.AsNoTracking() on subscription.PlanVersionInternalId equals version.Id
                where subscription.UserInternalId==actor.ActorId orderby subscription.SubscribedAt descending
                select new {subscription.SubscriptionId,plan.Name,version.PlanVersionId,version.VersionNumber,version.RegistrationFee,subscription.Status,subscription.SubscribedAt}).Take(200).ToListAsync(ct)));
        api.MapGet("/plans/{planId:guid}/versions",async(Guid planId,SubscriptionDbContext db,CancellationToken ct)=>
        {
            var now=DateTimeOffset.UtcNow;
            return Results.Ok(await (from version in db.PlanVersions.AsNoTracking() join plan in db.Plans.AsNoTracking() on version.PlanInternalId equals plan.Id
                where plan.PlanId==planId && version.Status=="Active" && version.EffectiveFrom<=now && (version.EffectiveTo==null || version.EffectiveTo>now)
                select new {version.PlanVersionId,version.VersionNumber,version.ChallengeCapital,version.RegistrationFee}).ToListAsync(ct));
        });
        api.MapPost("/subscriptions",async(SubscribeRequest request,SubscriptionDbContext db,ICurrentActor actor,CancellationToken ct)=>
        {
            var now=DateTimeOffset.UtcNow;
            var version=await db.PlanVersions.SingleOrDefaultAsync(x=>x.PlanVersionId==request.PlanVersionId && x.Status=="Active" && x.EffectiveFrom<=now && (x.EffectiveTo==null || x.EffectiveTo>now),ct);
            if(version==null) return Results.NotFound(new {message="Active plan version not found."});
            var subscription=new UserSubscription {SubscriptionId=Guid.NewGuid(),UserInternalId=actor.ActorId,PlanInternalId=version.PlanInternalId,PlanVersionInternalId=version.Id,SubscribedAt=now};
            db.Add(subscription);await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/subscriptions/{subscription.SubscriptionId}",new {subscription.SubscriptionId,subscription.Status});
        });
        api.MapGet("/payments",async(PaymentsDbContext db,ICurrentActor actor,CancellationToken ct)=>Results.Ok(await db.Payments.AsNoTracking().Where(x=>x.UserInternalId==actor.ActorId).OrderByDescending(x=>x.CreatedAt).Take(200).Select(x=>new {x.PaymentId,x.SubscriptionId,x.Amount,x.CurrencyCode,x.Status,x.PaidAt,x.CreatedAt}).ToListAsync(ct)));
        api.MapGet("/payments/{paymentId:guid}/receipt",async(Guid paymentId,PaymentsDbContext db,ICurrentActor actor,CancellationToken ct)=>
        {
            var receipt=await (from payment in db.Payments.AsNoTracking() join item in db.Receipts.AsNoTracking() on payment.Id equals item.PaymentInternalId where payment.PaymentId==paymentId && payment.UserInternalId==actor.ActorId
                select new {item.ReceiptId,item.ReceiptNumber,payment.PaymentId,payment.Amount,payment.CurrencyCode,payment.PaidAt,payment.ProviderPaymentId}).SingleOrDefaultAsync(ct);
            return receipt==null?Results.NotFound():Results.Ok(receipt);
        });
        api.MapPost("/payments/checkout",async(CheckoutRequest request,PaymentsDbContext db,SubscriptionDbContext subscriptions,RazorpayGateway gateway,ICurrentActor actor,IConfiguration config,CancellationToken ct)=>
        {
            if(!config.GetValue<bool>("Razorpay:CheckoutEnabled")) return Results.Problem("Checkout is disabled until challenge provisioning is ready.",statusCode:503);
            if(!gateway.IsConfigured) return Results.Problem("Razorpay is not configured.",statusCode:503);
            if(request.IdempotencyKey==Guid.Empty) return Results.BadRequest(new {message="Idempotency key is required."});
            var subscription=await subscriptions.Subscriptions.AsNoTracking().SingleOrDefaultAsync(x=>x.SubscriptionId==request.SubscriptionId && x.UserInternalId==actor.ActorId,ct);
            if(subscription==null) return Results.NotFound();
            if(subscription.Status!="PendingPayment") return Results.Conflict(new {message="Subscription is no longer awaiting payment."});
            var existing=await db.Payments.SingleOrDefaultAsync(x=>x.IdempotencyKey==request.IdempotencyKey,ct);
            if(existing!=null)
            {
                if(existing.UserInternalId!=actor.ActorId || existing.SubscriptionId!=request.SubscriptionId) return Results.Conflict();
                return CheckoutResult(existing,gateway);
            }
            if(await db.Payments.AnyAsync(x=>x.SubscriptionId==request.SubscriptionId && x.Status!="Failed",ct)) return Results.Conflict(new {message="A payment already exists for this subscription. Reconcile it before creating another."});
            var version=await subscriptions.PlanVersions.AsNoTracking().SingleAsync(x=>x.Id==subscription.PlanVersionInternalId,ct);
            var payment=new PaymentTransaction {PaymentId=Guid.NewGuid(),UserInternalId=actor.ActorId,SubscriptionId=subscription.SubscriptionId,Provider="Razorpay",Amount=version.RegistrationFee,IdempotencyKey=request.IdempotencyKey,Status="Creating"};
            PaymentSignature.ToPaise(payment.Amount);
            db.Add(payment);await db.SaveChangesAsync(ct);
            // Do not retry an uncertain external creation. The receipt identifies the intent for reconciliation.
            var order=await gateway.CreateOrderAsync(payment.PaymentId,payment.Amount,ct);
            payment.ProviderOrderId=order.Id;payment.Status="Pending";
            await db.SaveChangesAsync(CancellationToken.None);
            return CheckoutResult(payment,gateway);
        });
        api.MapPost("/payments/{paymentId:guid}/verify",async(Guid paymentId,VerifyPaymentRequest request,PaymentsDbContext db,RazorpayGateway gateway,ICurrentActor actor,CancellationToken ct)=>
        {
            var payment=await db.Payments.SingleOrDefaultAsync(x=>x.PaymentId==paymentId && x.UserInternalId==actor.ActorId,ct);
            if(payment?.ProviderOrderId==null) return Results.NotFound();
            if(!gateway.VerifyCheckout(payment.ProviderOrderId,request.PaymentId,request.Signature)) return Results.BadRequest(new {message="Payment signature is invalid."});
            var verified=await gateway.FetchPaymentAsync(request.PaymentId,ct);
            return await RecordPayment(payment,verified,db,ct);
        });
        app.MapPost("/api/v1/webhooks/razorpay",async(HttpRequest request,PaymentsDbContext db,RazorpayGateway gateway,CancellationToken ct)=>
        {
            if(request.ContentLength>65536) return Results.StatusCode(413);
            using var reader=new StreamReader(request.Body);
            var raw=await reader.ReadToEndAsync(ct);
            if(raw.Length>65536) return Results.StatusCode(413);
            if(!gateway.VerifyWebhook(raw,request.Headers["X-Razorpay-Signature"])) return Results.Unauthorized();
            using var json=JsonDocument.Parse(raw);
            if(json.RootElement.GetProperty("event").GetString()!="payment.captured") return Results.Ok();
            var entity=json.RootElement.GetProperty("payload").GetProperty("payment").GetProperty("entity");
            var orderId=entity.GetProperty("order_id").GetString();
            var payment=await db.Payments.SingleOrDefaultAsync(x=>x.ProviderOrderId==orderId,ct);
            if(payment==null) return Results.NotFound();
            var verified=await gateway.FetchPaymentAsync(entity.GetProperty("id").GetString()!,ct);
            return await RecordPayment(payment,verified,db,ct);
        }).AllowAnonymous();
    }
    private static IResult CheckoutResult(PaymentTransaction payment,RazorpayGateway gateway)=>payment.Status=="Pending" && payment.ProviderOrderId!=null
        ?Results.Ok(new {payment.PaymentId,orderId=payment.ProviderOrderId,keyId=gateway.KeyId,amount=PaymentSignature.ToPaise(payment.Amount),currency=payment.CurrencyCode})
        :Results.Conflict(new {message="Payment is processing or requires reconciliation.",payment.Status});
    private static async Task<IResult> RecordPayment(PaymentTransaction payment,VerifiedPayment verified,PaymentsDbContext db,CancellationToken ct)
    {
        if(verified.OrderId!=payment.ProviderOrderId || verified.Amount!=PaymentSignature.ToPaise(payment.Amount) || verified.Currency!=payment.CurrencyCode || verified.Status!="captured")
            return Results.BadRequest(new {message="Payment has not been captured for the expected order and amount."});
        if(payment.Status=="Paid") return payment.ProviderPaymentId==verified.Id?Results.Ok(new {payment.PaymentId,payment.Status}):Results.Conflict();
        payment.Status="Paid";payment.ProviderPaymentId=verified.Id;payment.PaidAt=DateTimeOffset.UtcNow;
        db.Add(new Receipt {ReceiptId=Guid.NewGuid(),PaymentInternalId=payment.Id,ReceiptNumber="MFX-"+payment.PaymentId.ToString("N")});
        // Receipt and captured state commit together; fulfilment remains a separate, explicitly pending workflow.
        await db.SaveChangesAsync(ct);
        return Results.Ok(new {payment.PaymentId,payment.Status,fulfilmentStatus="PendingChallengeProvisioning"});
    }
}
public sealed record SubscribeRequest(Guid PlanVersionId);
public sealed record CheckoutRequest(Guid SubscriptionId,Guid IdempotencyKey);
public sealed record VerifyPaymentRequest(string PaymentId,string Signature);
