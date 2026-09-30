using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.Intelligence;

public sealed class InstrumentSignal : EntityBase
{
    public Guid SignalId { get; set; }
    public string InstrumentToken { get; set; } = "";
    public DateTimeOffset SignalTime { get; set; }
    public string Direction { get; set; } = "Neutral";
    public string Strength { get; set; } = "Neutral";
    public decimal Confidence { get; set; }
    public decimal? ExpectedMoveMin { get; set; }
    public decimal? ExpectedMoveMax { get; set; }
    public decimal RiskScore { get; set; }
    public string? MarketRegime { get; set; }
    public string ModelVersion { get; set; } = "v1";
    public string ReasonsJson { get; set; } = "[]";
}
