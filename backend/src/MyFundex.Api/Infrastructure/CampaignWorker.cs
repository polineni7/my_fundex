using Microsoft.EntityFrameworkCore;
using MyFundex.Administration;
using MyFundex.Messaging.Contracts;
namespace MyFundex.Api.Infrastructure;

public sealed class CampaignWorker(IServiceScopeFactory scopes, IConfiguration config, ILogger<CampaignWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(ct))
        {
            if (!config.GetValue<bool>("Campaigns:SendingEnabled")) continue;
            if (!Uri.TryCreate(config["Campaigns:PublicBaseUrl"], UriKind.Absolute, out var baseUrl) || baseUrl.Scheme != "https") continue;
            try
            {
                using var scope = scopes.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AdministrationDbContext>(); var now = DateTimeOffset.UtcNow;
                var ids = await db.Deliveries.AsNoTracking().Where(x => x.Status == "Pending" && db.Campaigns.Any(c => c.CampaignId == x.CampaignId && c.Status == "Queued" && (c.ScheduledAt == null || c.ScheduledAt <= now))).OrderBy(x => x.Id).Take(25).Select(x => x.DeliveryId).ToListAsync(ct);
                foreach (var id in ids)
                {
                    using var deliveryScope = scopes.CreateScope(); var local = deliveryScope.ServiceProvider.GetRequiredService<AdministrationDbContext>();
                    var delivery = await local.Deliveries.SingleAsync(x => x.DeliveryId == id, ct); if (delivery.Status != "Pending") continue;
                    var campaign = await local.Campaigns.AsNoTracking().SingleOrDefaultAsync(x => x.CampaignId == delivery.CampaignId && x.Status == "Queued", ct); if (campaign == null) continue;
                    if (!await local.Audiences.AnyAsync(x => x.AudienceId == campaign.AudienceId, ct)) continue;
                    var member = await local.Members.AsNoTracking().SingleOrDefaultAsync(x => x.MemberId == delivery.MemberId, ct);
                    if (member == null || !member.MarketingConsent || member.UnsubscribedAt != null || await local.Members.IgnoreQueryFilters().AnyAsync(x => x.Email == delivery.Email && x.UnsubscribedAt != null, ct)) { delivery.Status = "Suppressed"; await local.SaveChangesAsync(ct); continue; }
                    delivery.Status = "Sending"; delivery.Attempts++; delivery.LeaseUntil = now.AddMinutes(5);
                    try { await local.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { continue; }
                    // No DB transaction spans SMTP. Ambiguous sends are never automatically retried.
                    try
                    {
                        var url = new Uri(baseUrl, $"/api/v1/marketing/unsubscribe/{member.UnsubscribeToken}");
                        await deliveryScope.ServiceProvider.GetRequiredService<IEmailTransport>().SendAsync(new(delivery.Email, campaign.Subject, campaign.Body + "\n\nUnsubscribe: " + url), ct);
                        delivery.Status = "Sent"; delivery.SentAt = DateTimeOffset.UtcNow; delivery.Error = null;
                    }
                    catch (Exception error) when (error is not OperationCanceledException)
                    {
                        delivery.Status = "ReviewRequired"; delivery.Error = "Delivery could not be confirmed. Check SMTP/provider logs before any resend."; logger.LogWarning("Campaign delivery {DeliveryId} requires manual review", id);
                    }
                    await local.SaveChangesAsync(CancellationToken.None);
                }
                await db.Deliveries.Where(x => x.Status == "Sending" && x.LeaseUntil < now).ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "ReviewRequired").SetProperty(x => x.Error, "Delivery confirmation interrupted; review provider logs before resending."), ct);
                await db.Campaigns.Where(c => c.Status == "Queued" && !db.Deliveries.Any(d => d.CampaignId == c.CampaignId && (d.Status == "Pending" || d.Status == "Sending"))).ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "Completed"), ct);
            }
            catch (Exception error) when (error is not OperationCanceledException) { logger.LogError(error, "Campaign delivery processing failed"); }
        }
    }
}
