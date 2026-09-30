using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Identity;

public sealed class Role : EntityBase
{
    public Guid RoleId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}
