using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.BuildingBlocks.Results;
using MyFundex.Contracts;

namespace MyFundex.Trading;

public sealed record PlaceOrderCommand(
    Guid AccountId,
    string InstrumentToken,
    string Symbol,
    string Side,
    string OrderType,
    decimal Quantity,
    decimal? Price,
    Guid IdempotencyKey
);
