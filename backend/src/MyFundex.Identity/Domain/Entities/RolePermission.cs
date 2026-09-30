using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Identity;

public sealed class RolePermission : EntityBase
{
    public long RoleInternalId { get; set; }
    public long PermissionInternalId { get; set; }
}
