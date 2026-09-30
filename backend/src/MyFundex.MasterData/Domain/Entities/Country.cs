using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.MasterData;

public sealed class Country : EntityBase
{
    public Guid CountryId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}
