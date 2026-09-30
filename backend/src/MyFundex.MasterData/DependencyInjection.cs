using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFundex.BuildingBlocks.Abstractions;
using MyFundex.BuildingBlocks.Domain;
using MyFundex.BuildingBlocks.Persistence;

namespace MyFundex.MasterData;

public static class MasterDataModule
{
    public static IServiceCollection AddMasterDataModule(this IServiceCollection s, string cs)
    {
        s.AddDbContext<MasterDataDbContext>(o => o.UseNpgsql(cs));
        return s;
    }
}
