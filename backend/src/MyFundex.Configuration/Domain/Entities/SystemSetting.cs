using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;
using MyFundex.Contracts;

namespace MyFundex.Configuration;

public sealed class SystemSetting : EntityBase
{
    public Guid SettingId { get; set; }
    public string SettingKey { get; set; } = "";
    public string SettingValue { get; set; } = "";
    public string ValueType { get; set; } = "String";
    public string Category { get; set; } = "General";
    public string Environment { get; set; } = "UAT";
    public bool IsSensitive { get; set; }
    public string? Description { get; set; }
}
