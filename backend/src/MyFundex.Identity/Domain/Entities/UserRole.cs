using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Identity;

public sealed class UserRole : EntityBase
{
    public long UserInternalId { get; set; }
    public long RoleInternalId { get; set; }
}
