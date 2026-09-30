using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Payments;

public sealed class Invoice : EntityBase
{
    public Guid InvoiceId { get; set; }
    public long PaymentInternalId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrossAmount { get; set; }
    public string? StorageKey { get; set; }
}
