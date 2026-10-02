using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using MyFundex.Administration;
using MyFundex.Contracts;
namespace MyFundex.Api.Infrastructure;

public static class EngagementEndpoints
{
    public static void MapEngagement(this WebApplication app)
    {
        var admin = app.MapGroup("/api/v1/admin/engagement").RequireAuthorization(p => p.RequireRole("ADMIN"));
        admin.MapGet("/audiences", async (AdministrationDbContext db, CancellationToken ct) => Results.Ok(await db.Audiences.AsNoTracking().OrderByDescending(x => x.Id).Take(500).ToListAsync(ct)));
        admin.MapPost("/audiences", (AudienceInput input, AdministrationDbContext db, IAuditWriter audit, CancellationToken ct) => SaveAudience(null, input, db, audit, ct));
        admin.MapPut("/audiences/{id:guid}", (Guid id, AudienceInput input, AdministrationDbContext db, IAuditWriter audit, CancellationToken ct) => SaveAudience(id, input, db, audit, ct));
        admin.MapGet("/audiences/{id:guid}/members", async (Guid id, AdministrationDbContext db, CancellationToken ct) => Results.Ok(await db.Members.AsNoTracking().Where(x => x.AudienceId == id).OrderByDescending(x => x.Id).Take(1000).Select(x => new { x.MemberId, x.Email, x.Name, x.MarketingConsent, x.ConsentSource, x.ConsentedAt, x.UnsubscribedAt, x.Version }).ToListAsync(ct)));
        admin.MapPost("/audiences/{id:guid}/members", async (Guid id, MemberInput input, AdministrationDbContext db, CancellationToken ct) =>
        {
            if (!await db.Audiences.AnyAsync(x => x.AudienceId == id, ct)) return Results.NotFound();
            if (string.IsNullOrWhiteSpace(input.Email) || input.Email.Length > 254 || !MailAddress.TryCreate(input.Email, out var email) || email.Address != input.Email.Trim() || (input.Name == null || input.Name.Length > 150) || (input.ConsentSource == null || input.ConsentSource.Length > 250) ||
                (input.MarketingConsent && string.IsNullOrWhiteSpace(input.ConsentSource))) return Results.BadRequest(new { message = "Enter a valid email and record the consent source when opting in." });
            if (input.MarketingConsent && await db.Members.IgnoreQueryFilters().AnyAsync(x => x.Email == input.Email.Trim().ToLowerInvariant() && x.UnsubscribedAt != null, ct))
                return Results.Conflict(new { message = "This address previously unsubscribed. A new verified opt-in flow is required before adding it to marketing." });
            var member = new AudienceMember { AudienceId = id, Email = input.Email.Trim().ToLowerInvariant(), Name = input.Name.Trim(), MarketingConsent = input.MarketingConsent, ConsentSource = input.ConsentSource.Trim(), ConsentedAt = input.MarketingConsent ? DateTimeOffset.UtcNow : null };
            db.Add(member); await db.SaveChangesAsync(ct); return Results.Ok(new { member.MemberId });
        });
        admin.MapDelete("/members/{id:guid}", async (Guid id, long version, AdministrationDbContext db, CancellationToken ct) =>
        {
            var item = await db.Members.SingleOrDefaultAsync(x => x.MemberId == id, ct); if (item == null) return Results.NotFound(); if (item.Version != version) return Results.Conflict();
            db.Remove(item); await db.SaveChangesAsync(ct); return Results.NoContent();
        });
        admin.MapDelete("/audiences/{id:guid}", async (Guid id, long version, AdministrationDbContext db, CancellationToken ct) =>
        {
            var item = await db.Audiences.SingleOrDefaultAsync(x => x.AudienceId == id, ct); if (item == null) return Results.NotFound(); if (item.Version != version) return Results.Conflict();
            if (await db.Campaigns.AnyAsync(x => x.AudienceId == id && x.Status == "Queued", ct)) return Results.Conflict(new { message = "Cancel queued campaigns before archiving this audience." });
            db.Remove(item); await db.SaveChangesAsync(ct); return Results.NoContent();
        });
        admin.MapPost("/audiences/{id:guid}/import-traders", async (Guid id, ImportAudienceInput input, AdministrationDbContext db, MyFundex.Identity.IdentityDbContext identity, CancellationToken ct) =>
        {
            if (input.From >= input.To) return Results.BadRequest(new { message = "Select a valid registration date range." });
            if (!await db.Audiences.AnyAsync(x => x.AudienceId == id, ct)) return Results.NotFound();
            var emails = await identity.Users.AsNoTracking().Where(u => u.CreatedAt >= input.From && u.CreatedAt < input.To && identity.UserRoles.Any(link => link.UserInternalId == u.Id && identity.Roles.Any(role => role.Id == link.RoleInternalId && role.Code == "TRADER"))).OrderBy(u => u.Id).Take(1001).Select(u => u.Email).ToListAsync(ct);
            if (emails.Count > 1000) return Results.BadRequest(new { message = "Narrow the date range to at most 1000 traders." });
            var existing = await db.Members.IgnoreQueryFilters().Where(x => x.AudienceId == id).Select(x => x.Email).ToListAsync(ct); var seen = existing.ToHashSet(StringComparer.OrdinalIgnoreCase); var added = 0;
            foreach (var email in emails) if (seen.Add(email)) { db.Add(new AudienceMember { AudienceId = id, Email = email, MarketingConsent = false, ConsentSource = "Imported registration; consent not inferred" }); added++; }
            await db.SaveChangesAsync(ct); return Results.Ok(new { added, message = "Imported without marketing permission. Record verified consent before sending." });
        });
        admin.MapPut("/members/{id:guid}", async (Guid id, MemberUpdateInput input, AdministrationDbContext db, CancellationToken ct) =>
        {
            var item = await db.Members.SingleOrDefaultAsync(x => x.MemberId == id, ct); if (item == null) return Results.NotFound(); if (item.Version != input.Version) return Results.Conflict();
            if (string.IsNullOrWhiteSpace(input.ConsentSource) || input.ConsentSource.Length > 250) return Results.BadRequest(new { message = "Record the consent source." });
            if (input.MarketingConsent && await db.Members.IgnoreQueryFilters().AnyAsync(x => x.Email == item.Email && x.UnsubscribedAt != null, ct)) return Results.Conflict(new { message = "Previously unsubscribed addresses require a new verified opt-in flow." });
            item.MarketingConsent = input.MarketingConsent; item.ConsentSource = input.ConsentSource.Trim(); item.ConsentedAt = input.MarketingConsent ? DateTimeOffset.UtcNow : null; await db.SaveChangesAsync(ct); return Results.NoContent();
        });
        admin.MapGet("/campaigns", async (AdministrationDbContext db, CancellationToken ct) => Results.Ok(await db.Campaigns.AsNoTracking().OrderByDescending(x => x.Id).Take(500).ToListAsync(ct)));
        admin.MapPost("/campaigns", (CampaignInput input, AdministrationDbContext db, IAuditWriter audit, CancellationToken ct) => SaveCampaign(null, input, db, audit, ct));
        admin.MapPut("/campaigns/{id:guid}", (Guid id, CampaignInput input, AdministrationDbContext db, IAuditWriter audit, CancellationToken ct) => SaveCampaign(id, input, db, audit, ct));
        admin.MapGet("/campaigns/{id:guid}/deliveries", async (Guid id, AdministrationDbContext db, CancellationToken ct) => Results.Ok(await db.Deliveries.AsNoTracking().Where(x => x.CampaignId == id).OrderBy(x => x.Id).Take(1000).Select(x => new { x.DeliveryId, x.Email, x.Status, x.Attempts, x.SentAt, x.Error }).ToListAsync(ct)));
        admin.MapPost("/campaigns/{id:guid}/queue", async (Guid id, VersionInput input, AdministrationDbContext db, IConfiguration config, IAuditWriter audit, CancellationToken ct) =>
        {
            if (!config.GetValue<bool>("Campaigns:SendingEnabled")) return Results.Problem("Campaign sending is disabled. Configure SMTP and the public unsubscribe URL before enabling it.", statusCode: 503);
            if (!Uri.TryCreate(config["Campaigns:PublicBaseUrl"], UriKind.Absolute, out var publicUrl) || publicUrl.Scheme != "https")
                return Results.BadRequest(new { message = "Configure the HTTPS public unsubscribe URL before queueing campaigns." });
            var item = await db.Campaigns.SingleOrDefaultAsync(x => x.CampaignId == id, ct); if (item == null) return Results.NotFound();
            if (item.Version != input.Version || item.Status != "Draft") return Results.Conflict(new { message = "Only an unchanged draft can be queued." });
            var members = await db.Members.AsNoTracking().Where(x => x.AudienceId == item.AudienceId && x.MarketingConsent && x.UnsubscribedAt == null).Take(1001).ToListAsync(ct);
            if (members.Count == 0 || members.Count > 1000) return Results.BadRequest(new { message = "Choose an audience with 1–1000 opted-in members." });
            foreach (var member in members) db.Add(new CampaignDelivery { CampaignId = id, MemberId = member.MemberId, Email = member.Email });
            item.Status = "Queued"; item.QueuedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct);
            await audit.WriteAsync("Administration", "Campaign", id.ToString(), "Queued", null, $"{members.Count} recipients", ct); return Results.Ok(new { recipients = members.Count });
        });
        admin.MapPost("/campaigns/{id:guid}/cancel", async (Guid id, VersionInput input, AdministrationDbContext db, CancellationToken ct) =>
        {
            var item = await db.Campaigns.SingleOrDefaultAsync(x => x.CampaignId == id, ct); if (item == null) return Results.NotFound(); if (item.Version != input.Version) return Results.Conflict();
            item.Status = "Cancelled"; await db.SaveChangesAsync(ct); return Results.NoContent();
        });
        admin.MapDelete("/campaigns/{id:guid}", async (Guid id, long version, AdministrationDbContext db, CancellationToken ct) =>
        {
            var item = await db.Campaigns.SingleOrDefaultAsync(x => x.CampaignId == id, ct); if (item == null) return Results.NotFound(); if (item.Version != version || item.Status != "Draft") return Results.Conflict(new { message = "Only drafts may be deleted. Cancel queued campaigns to preserve delivery history." });
            db.Remove(item); await db.SaveChangesAsync(ct); return Results.NoContent();
        });
        admin.MapGet("/content", async (AdministrationDbContext db, CancellationToken ct) => Results.Ok(await db.Content.AsNoTracking().OrderByDescending(x => x.Id).Take(500).ToListAsync(ct)));
        admin.MapPost("/content", (ContentInput input, AdministrationDbContext db, IAuditWriter audit, CancellationToken ct) => SaveContent(null, input, db, audit, ct));
        admin.MapPut("/content/{id:guid}", (Guid id, ContentInput input, AdministrationDbContext db, IAuditWriter audit, CancellationToken ct) => SaveContent(id, input, db, audit, ct));
        admin.MapDelete("/content/{id:guid}", async (Guid id, long version, AdministrationDbContext db, CancellationToken ct) =>
        {
            var item = await db.Content.SingleOrDefaultAsync(x => x.ContentId == id, ct); if (item == null) return Results.NotFound(); if (item.Version != version) return Results.Conflict(); db.Remove(item); await db.SaveChangesAsync(ct); return Results.NoContent();
        });
        app.MapGet("/api/v1/content/{slug}", async (string slug, AdministrationDbContext db, CancellationToken ct) =>
        {
            var now = DateTimeOffset.UtcNow;
            var item = await db.Content.AsNoTracking().Where(x => x.Slug == slug && x.Status == "Published" && (x.StartsAt == null || x.StartsAt <= now) && (x.EndsAt == null || x.EndsAt > now)).Select(x => new { x.Kind, x.Slug, x.Title, x.Body, x.StartsAt, x.EndsAt }).SingleOrDefaultAsync(ct);
            return item == null ? Results.NotFound() : Results.Ok(item);
        }).AllowAnonymous();
        app.MapGet("/api/v1/marketing/unsubscribe/{token:guid}", (Guid token) => Results.Content($"<html><body><h1>Unsubscribe from offers</h1><form method='post' action='/api/v1/marketing/unsubscribe/{token}'><button>Confirm unsubscribe</button></form></body></html>", "text/html")).AllowAnonymous();
        app.MapPost("/api/v1/marketing/unsubscribe/{token:guid}", async (Guid token, AdministrationDbContext db, CancellationToken ct) =>
        {
            var member = await db.Members.SingleOrDefaultAsync(x => x.UnsubscribeToken == token, ct);
            if (member != null) { var matches = await db.Members.Where(x => x.Email == member.Email).ToListAsync(ct); foreach (var match in matches) { match.MarketingConsent = false; match.UnsubscribedAt = DateTimeOffset.UtcNow; } await db.SaveChangesAsync(ct); }
            return Results.Content("You have been unsubscribed from marketing messages.", "text/plain");
        }).AllowAnonymous();
    }
    private static async Task<IResult> SaveAudience(Guid? id, AudienceInput input, AdministrationDbContext db, IAuditWriter audit, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Length > 150 || (input.Description == null || input.Description.Length > 1000)) return Results.BadRequest(new { message = "Provide a name up to 150 characters and description up to 1000." });
        var item = id.HasValue ? await db.Audiences.SingleOrDefaultAsync(x => x.AudienceId == id, ct) : new Audience(); if (item == null) return Results.NotFound(); if (id.HasValue && item.Version != input.Version) return Results.Conflict();
        item.Name = input.Name.Trim(); item.Description = input.Description.Trim(); if (!id.HasValue) db.Add(item); await db.SaveChangesAsync(ct); await audit.WriteAsync("Administration", "Audience", item.AudienceId.ToString(), "Saved", null, item.Name, ct); return Results.Ok(item);
    }
    private static async Task<IResult> SaveCampaign(Guid? id, CampaignInput input, AdministrationDbContext db, IAuditWriter audit, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Length > 150 || string.IsNullOrWhiteSpace(input.Subject) || input.Subject.Length > 200 || input.Subject.Contains('\r') || input.Subject.Contains('\n') || string.IsNullOrWhiteSpace(input.Body) || (input.Body == null || input.Body.Length > 20000) || !await db.Audiences.AnyAsync(x => x.AudienceId == input.AudienceId, ct)) return Results.BadRequest(new { message = "Provide a name, single-line subject, message and saved audience." });
        var item = id.HasValue ? await db.Campaigns.SingleOrDefaultAsync(x => x.CampaignId == id, ct) : new Campaign(); if (item == null) return Results.NotFound(); if (id.HasValue && (item.Version != input.Version || item.Status != "Draft")) return Results.Conflict();
        item.Name = input.Name.Trim(); item.Subject = input.Subject.Trim(); item.Body = input.Body; item.AudienceId = input.AudienceId; item.ScheduledAt = input.ScheduledAt; if (!id.HasValue) db.Add(item); await db.SaveChangesAsync(ct); await audit.WriteAsync("Administration", "Campaign", item.CampaignId.ToString(), "DraftSaved", null, item.Name, ct); return Results.Ok(item);
    }
    private static async Task<IResult> SaveContent(Guid? id, ContentInput input, AdministrationDbContext db, IAuditWriter audit, CancellationToken ct)
    {
        if (input.Kind is not ("Page" or "Banner" or "FAQ" or "Event") || input.Status is not ("Draft" or "Published") || string.IsNullOrWhiteSpace(input.Slug) || input.Slug.Length > 100 || input.Slug.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-') || string.IsNullOrWhiteSpace(input.Title) || input.Title.Length > 200 || (input.Body == null || input.Body.Length > 20000) || input.StartsAt >= input.EndsAt) return Results.BadRequest(new { message = "Check content type, title, slug, publication status and date range." });
        var item = id.HasValue ? await db.Content.SingleOrDefaultAsync(x => x.ContentId == id, ct) : new ContentEntry(); if (item == null) return Results.NotFound(); if (id.HasValue && item.Version != input.Version) return Results.Conflict();
        item.Kind = input.Kind; item.Slug = input.Slug.Trim().ToLowerInvariant(); item.Title = input.Title.Trim(); item.Body = input.Body; item.Status = input.Status; item.StartsAt = input.StartsAt; item.EndsAt = input.EndsAt; if (!id.HasValue) db.Add(item); await db.SaveChangesAsync(ct); await audit.WriteAsync("Administration", "Content", item.ContentId.ToString(), "Saved", null, item.Status, ct); return Results.Ok(item);
    }
}
public sealed record VersionInput(long Version);
public sealed record AudienceInput(string Name, string Description, long Version);
public sealed record MemberInput(string Email, string Name, bool MarketingConsent, string ConsentSource);
public sealed record CampaignInput(string Name, string Subject, string Body, Guid AudienceId, DateTimeOffset? ScheduledAt, long Version);
public sealed record ContentInput(string Kind, string Slug, string Title, string Body, string Status, DateTimeOffset? StartsAt, DateTimeOffset? EndsAt, long Version);

public sealed record ImportAudienceInput(DateTimeOffset From, DateTimeOffset To);
public sealed record MemberUpdateInput(bool MarketingConsent, string ConsentSource, long Version);
