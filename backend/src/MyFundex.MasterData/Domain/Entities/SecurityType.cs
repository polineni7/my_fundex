using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.MasterData;

public sealed class SecurityType : EntityBase
{
    public Guid SecurityTypeId { get; set; }
    public string Code { get; set; } = "EQUITY";
    public string Name { get; set; } = "Equity";
}
