using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Payments;

public sealed class Receipt : EntityBase, IImmutableRecord
{
    public Guid ReceiptId { get; set; }
    public long PaymentInternalId { get; set; }
    public string ReceiptNumber { get; set; } = "";
    public string? StorageKey { get; set; }
}
