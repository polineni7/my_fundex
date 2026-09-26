using System.Security.Claims;using MyFundex.BuildingBlocks.Abstractions;
namespace MyFundex.Api.Infrastructure;
public sealed class CurrentActor(IHttpContextAccessor http):ICurrentActor{public long ActorId{get{var value=http.HttpContext?.User.FindFirstValue("internal_user_id");return long.TryParse(value,out var id)?id:1;}}}
