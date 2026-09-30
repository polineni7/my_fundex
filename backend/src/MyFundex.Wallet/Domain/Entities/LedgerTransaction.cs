using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Wallet;

public sealed class LedgerTransaction : EntityBase, IImmutableRecord
{
    public Guid TransactionId { get; set; }
    public string TransactionType { get; set; } = "";
    public string ReferenceType { get; set; } = "";
    public string ReferenceId { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
