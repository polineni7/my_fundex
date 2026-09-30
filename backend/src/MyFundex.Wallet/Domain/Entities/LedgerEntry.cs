using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Wallet;

public sealed class LedgerEntry : EntityBase, IImmutableRecord
{
    public Guid EntryId { get; set; }
    public long WalletInternalId { get; set; }
    public long TransactionInternalId { get; set; }
    public string Direction { get; set; } = "Credit";
    public decimal Amount { get; set; }
}
