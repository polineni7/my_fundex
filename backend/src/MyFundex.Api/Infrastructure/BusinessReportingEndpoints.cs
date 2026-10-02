using Microsoft.EntityFrameworkCore;
using MyFundex.Identity;
using MyFundex.Subscription;
using MyFundex.Payments;
using MyFundex.FundedAccounts;

namespace MyFundex.Api.Infrastructure;

public static class BusinessReportingEndpoints
{
    public static void MapBusinessReporting(this WebApplication app)
    {
        app.MapGet("/api/v1/admin/business-report", async (DateTimeOffset? from, DateTimeOffset? to,
            IdentityDbContext identity, SubscriptionDbContext subscriptions, PaymentsDbContext payments,
            AccountsDbContext accounts, CancellationToken ct) =>
        {
            if (from > to) return Results.BadRequest(new { message = "Start date must be before end date." });
            var traders = identity.Users.AsNoTracking().Where(u => identity.UserRoles.Any(link => link.UserInternalId == u.Id &&
                identity.Roles.Any(role => role.Id == link.RoleInternalId && role.Code == "TRADER")));
            var registered = traders.Where(u => (!from.HasValue || u.CreatedAt >= from) && (!to.HasValue || u.CreatedAt < to));
            var enrollments = subscriptions.Subscriptions.AsNoTracking().Where(x => (!from.HasValue || x.SubscribedAt >= from) && (!to.HasValue || x.SubscribedAt < to));
            var statusCounts = await enrollments.GroupBy(x => x.Status).Select(g => new { status = g.Key, count = g.Count() }).ToListAsync(ct);
            var paid = payments.Payments.AsNoTracking().Where(x => x.Status == "Paid" && x.CurrencyCode == "INR" &&
                (!from.HasValue || x.PaidAt >= from) && (!to.HasValue || x.PaidAt < to));
            var collectedFees = await paid.SumAsync(x => (decimal?)x.Amount, ct) ?? 0;
            var active = accounts.Accounts.AsNoTracking().Where(x => x.Status == "Active");
            var capital = await active.GroupBy(x => x.TradingMode).Select(g => new { mode = g.Key, amount = g.Sum(x => x.FundedCapital), accounts = g.Count() }).ToListAsync(ct);
            var users = await registered.OrderByDescending(x => x.CreatedAt).Take(200).Select(x => new { x.Id, x.UserId, x.Email, x.Status, x.CreatedAt }).ToListAsync(ct);
            var ids = users.Select(x => x.Id).ToArray();
            var attempts = await (from attempt in subscriptions.Subscriptions.AsNoTracking()
                join plan in subscriptions.Plans.AsNoTracking() on attempt.PlanInternalId equals plan.Id
                where ids.Contains(attempt.UserInternalId)
                select new { attempt.UserInternalId, attempt.SubscriptionId, plan = plan.Name, attempt.Status, attempt.SubscribedAt, attempt.CompletedAt }).ToListAsync(ct);
            var lookup = attempts.ToLookup(x => x.UserInternalId);
            return Results.Ok(new { registrations = await registered.CountAsync(ct), collectedFees, statusCounts, capital,
                traders = users.Select(u => new { u.UserId, u.Email, u.Status, registeredAt = u.CreatedAt, assessments = lookup[u.Id].OrderByDescending(x => x.SubscribedAt) }),
                limit = 200 });
        }).RequireAuthorization(p => p.RequireRole("ADMIN"));
    }
}
