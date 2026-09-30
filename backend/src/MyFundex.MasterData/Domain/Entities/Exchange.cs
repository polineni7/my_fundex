using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.MasterData;

public sealed class Exchange : EntityBase
{
    public Guid ExchangeId { get; set; }
    public string Code { get; set; } = "NSE";
    public string Name { get; set; } = "National Stock Exchange";
}
