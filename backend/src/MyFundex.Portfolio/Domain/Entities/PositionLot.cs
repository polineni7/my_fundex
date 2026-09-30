using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Portfolio;

public sealed class PositionLot : EntityBase
{
    public Guid LotId { get; set; }
    public long PositionInternalId { get; set; }
    public long ExecutionInternalId { get; set; }
    public decimal OriginalQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateTimeOffset AcquiredAt { get; set; }
    public DateTimeOffset? MustExitBy { get; set; }
}
