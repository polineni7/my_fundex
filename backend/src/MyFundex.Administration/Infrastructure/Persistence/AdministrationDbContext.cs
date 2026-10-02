using Microsoft.EntityFrameworkCore;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Persistence;
namespace MyFundex.Administration;
public sealed class AdministrationDbContext(DbContextOptions<AdministrationDbContext> options, ICurrentActor actor) : AuditableDbContext(options,actor)
{
    public DbSet<Audience> Audiences => Set<Audience>();
    public DbSet<AudienceMember> Members => Set<AudienceMember>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignDelivery> Deliveries => Set<CampaignDelivery>();
    public DbSet<ContentEntry> Content => Set<ContentEntry>();
    protected override void OnModelCreating(ModelBuilder model) => ConfigureModel(model);
    public static void ConfigureModel(ModelBuilder m)
    {
        m.HasDefaultSchema("fundex_administration");
        ConfigureEntity(m.Entity<Audience>()); ConfigureEntity(m.Entity<AudienceMember>());
        ConfigureEntity(m.Entity<Campaign>()); ConfigureEntity(m.Entity<CampaignDelivery>()); ConfigureEntity(m.Entity<ContentEntry>());
        m.Entity<Audience>().ToTable("Audiences","fundex_administration").HasIndex(x=>x.AudienceId).IsUnique();
        m.Entity<AudienceMember>().ToTable("AudienceMembers","fundex_administration").HasIndex(x=>x.MemberId).IsUnique();
        m.Entity<AudienceMember>().HasIndex(x=>new{x.AudienceId,x.Email}).IsUnique();
        m.Entity<AudienceMember>().HasIndex(x=>x.UnsubscribeToken).IsUnique();
        m.Entity<Campaign>().ToTable("Campaigns","fundex_administration").HasIndex(x=>x.CampaignId).IsUnique();
        m.Entity<CampaignDelivery>().ToTable("CampaignDeliveries","fundex_administration").HasIndex(x=>x.DeliveryId).IsUnique();
        m.Entity<CampaignDelivery>().HasIndex(x=>new{x.CampaignId,x.MemberId}).IsUnique();
        m.Entity<CampaignDelivery>().HasIndex(x=>new{x.Status,x.LeaseUntil});
        m.Entity<ContentEntry>().ToTable("ContentEntries","fundex_administration").HasIndex(x=>x.ContentId).IsUnique();
        m.Entity<ContentEntry>().HasIndex(x=>x.Slug).IsUnique();
    }
}
