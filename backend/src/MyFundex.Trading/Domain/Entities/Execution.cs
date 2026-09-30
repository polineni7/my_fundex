using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.BuildingBlocks.Results;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public sealed class Execution : EntityBase, IImmutableRecord
{
    public Guid ExecutionId { get; set; }
    public long OrderInternalId { get; set; }
    public string? BrokerExecutionId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal GrossValue { get; set; }
    public DateTimeOffset ExecutedAt { get; set; }
}
