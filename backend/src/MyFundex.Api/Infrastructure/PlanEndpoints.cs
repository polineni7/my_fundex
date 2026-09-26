using Microsoft.EntityFrameworkCore;
using MyFundex.Subscription;
using MyFundex.Contracts;
namespace MyFundex.Api.Infrastructure;

public static class PlanEndpoints
{
    public static void MapPlanEndpoints(this WebApplication app)
    {
        var admin=app.MapGroup("/api/v1/admin/plans").RequireAuthorization(p=>p.RequireAssertion(c=>c.User.IsInRole("ADMIN") || c.User.IsInRole("MANAGER") && c.User.HasClaim("permission","plans.write")));
        admin.MapGet("/",async(SubscriptionDbContext db,CancellationToken ct)=>Results.Ok(await db.Plans.AsNoTracking().OrderBy(x=>x.Name).Select(x=>new {x.PlanId,x.Code,x.Name,x.Description}).ToListAsync(ct)));
        admin.MapPost("/",async(CreatePlanRequest request,SubscriptionDbContext db,IAuditWriter audit,CancellationToken ct)=>
        {
            if(string.IsNullOrWhiteSpace(request.Code)||string.IsNullOrWhiteSpace(request.Name)||request.Code.Length>50||request.Name.Length>150) return Results.BadRequest(new {message="A code (up to 50 characters) and name (up to 150 characters) are required."});
            var code=request.Code.Trim().ToUpperInvariant();
            if(await db.Plans.AnyAsync(x=>x.Code==code,ct)) return Results.Conflict(new {message="Plan code already exists."});
            var plan=new Plan {PlanId=Guid.NewGuid(),Code=code,Name=request.Name.Trim(),Description=request.Description};
            db.Add(plan);await db.SaveChangesAsync(ct);
            await audit.WriteAsync("Subscription","Plan",plan.PlanId.ToString(),"Created",null,plan.Name,ct);
            return Results.Created($"/api/v1/plans/{plan.PlanId}",new {plan.PlanId});
        });
        admin.MapPost("/{planId:guid}/versions",async(Guid planId,CreatePlanVersionRequest request,SubscriptionDbContext db,IAuditWriter audit,CancellationToken ct)=>
        {
            var plan=await db.Plans.SingleOrDefaultAsync(x=>x.PlanId==planId,ct);
            if(plan==null) return Results.NotFound();
            if(request.ChallengeCapital<=0 || request.RegistrationFee<1 || decimal.Round(request.RegistrationFee,2)!=request.RegistrationFee || request.Stages is null || request.Stages.Length==0 || request.Stages.Any(x=>string.IsNullOrWhiteSpace(x.Name)||x.StartingCapital<=0||x.PolicySetId==Guid.Empty))
                return Results.BadRequest(new {message="Provide positive capital, a fee in whole paise, and at least one named stage with a policy."});
            await using var transaction=await db.Database.BeginTransactionAsync(ct);
            var next=(await db.PlanVersions.Where(x=>x.PlanInternalId==plan.Id).MaxAsync(x=>(int?)x.VersionNumber,ct)??0)+1;
            var version=new PlanVersion {PlanVersionId=Guid.NewGuid(),PlanInternalId=plan.Id,VersionNumber=next,ChallengeCapital=request.ChallengeCapital,RegistrationFee=request.RegistrationFee,Status="Draft",EffectiveFrom=DateTimeOffset.UtcNow};
            db.Add(version);await db.SaveChangesAsync(ct);
            for(var index=0;index<request.Stages.Length;index++)
            {
                var stage=request.Stages[index];
                db.Add(new PlanStageDefinition {StageId=Guid.NewGuid(),PlanVersionInternalId=version.Id,StageNumber=index+1,Name=stage.Name.Trim(),StartingCapital=stage.StartingCapital,PolicySetId=stage.PolicySetId});
            }
            await db.SaveChangesAsync(ct);await transaction.CommitAsync(ct);
            await audit.WriteAsync("Subscription","PlanVersion",version.PlanVersionId.ToString(),"DraftCreated",null,null,ct);
            return Results.Created($"/api/v1/plans/{planId}/versions",new {version.PlanVersionId,version.VersionNumber,version.Status});
        });
    }
}
public sealed record CreatePlanRequest(string Code,string Name,string? Description);
public sealed record CreatePlanVersionRequest(decimal ChallengeCapital,decimal RegistrationFee,StageRequest[] Stages);
public sealed record StageRequest(string Name,decimal StartingCapital,Guid PolicySetId);
