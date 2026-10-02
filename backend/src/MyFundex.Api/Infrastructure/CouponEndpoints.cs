using Microsoft.EntityFrameworkCore;
using MyFundex.Payments;
using MyFundex.Contracts;
namespace MyFundex.Api.Infrastructure;

public static class CouponEndpoints
{
    public static void MapCoupons(this WebApplication app)
    {
        var routes = app.MapGroup("/api/v1/admin/coupons").RequireAuthorization(p => p.RequireRole("ADMIN"));
        routes.MapGet("", async (PaymentsDbContext db, CancellationToken ct) => Results.Ok(await db.Coupons.AsNoTracking().OrderByDescending(x => x.Id).Take(500).ToListAsync(ct)));
        routes.MapPost("", (CouponInput input, PaymentsDbContext db, IAuditWriter audit, CancellationToken ct) => Save(null, input, db, audit, ct));
        routes.MapPut("/{id:guid}", (Guid id, CouponInput input, PaymentsDbContext db, IAuditWriter audit, CancellationToken ct) => Save(id, input, db, audit, ct));
        routes.MapDelete("/{id:guid}", async (Guid id, long version, PaymentsDbContext db, IAuditWriter audit, CancellationToken ct) =>
        {
            var item = await db.Coupons.SingleOrDefaultAsync(x => x.CouponId == id, ct);
            if (item == null) return Results.NotFound();
            if (item.Version != version) return Results.Conflict(new { message = "Coupon changed. Refresh first." });
            db.Remove(item); await db.SaveChangesAsync(ct);
            await audit.WriteAsync("Payments", "Coupon", id.ToString(), "Archived", null, item.Code, ct);
            return Results.NoContent();
        });
    }
    private static async Task<IResult> Save(Guid? id, CouponInput input, PaymentsDbContext db, IAuditWriter audit, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Code) || input.Code.Length > 40 || input.Code.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-') ||
            string.IsNullOrWhiteSpace(input.Name) || input.Name.Length > 150 || input.StartsAt >= input.EndsAt || input.MaximumUses < 1 || input.UsesPerUser < 1 || input.UsesPerUser > input.MaximumUses || input.MinimumFee < 0)
            return Results.BadRequest(new { message = "Provide a code, name, valid dates and positive redemption limits." });
        try { CouponPricing.Discount(input.DiscountType, input.Value, 1000); } catch (ArgumentException) { return Results.BadRequest(new { message = "Choose fixed INR or percentage discount and a valid positive amount (percentage at most 100)." }); }
        var item = id.HasValue ? await db.Coupons.SingleOrDefaultAsync(x => x.CouponId == id, ct) : new Coupon();
        if (item == null) return Results.NotFound();
        if (id.HasValue && item.Version != input.Version) return Results.Conflict(new { message = "Coupon changed. Refresh first." });
        if (item.ReservedUses > 0 && (item.Code != input.Code.Trim().ToUpperInvariant() || item.Value != input.Value || item.DiscountType != input.DiscountType || item.PlanId != input.PlanId || item.UserId != input.UserId))
            return Results.Conflict(new { message = "Redeemed coupon pricing and eligibility cannot change. Create a new coupon." });
        if (input.MaximumUses < item.ReservedUses) return Results.BadRequest(new { message = "Maximum uses cannot be lower than reserved uses." });
        item.Code = input.Code.Trim().ToUpperInvariant(); item.Name = input.Name.Trim(); item.DiscountType = input.DiscountType; item.Value = input.Value;
        item.MinimumFee = input.MinimumFee; item.PlanId = input.PlanId; item.UserId = input.UserId; item.StartsAt = input.StartsAt; item.EndsAt = input.EndsAt;
        item.MaximumUses = input.MaximumUses; item.UsesPerUser = input.UsesPerUser; item.IsActive = input.IsActive;
        if (!id.HasValue) db.Add(item); await db.SaveChangesAsync(ct);
        await audit.WriteAsync("Payments", "Coupon", item.CouponId.ToString(), id.HasValue ? "Updated" : "Created", null, item.Code, ct);
        return Results.Ok(item);
    }
}
public sealed record CouponInput(string Code, string Name, string DiscountType, decimal Value, decimal MinimumFee, Guid? PlanId, Guid? UserId, DateTimeOffset StartsAt, DateTimeOffset EndsAt, int MaximumUses, int UsesPerUser, bool IsActive, long Version);
