using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
namespace MyFundex.Administration;
public static class AdministrationModule
{
    public static IServiceCollection AddAdministrationModule(this IServiceCollection services,string connection) =>
        services.AddDbContext<AdministrationDbContext>(o=>o.UseNpgsql(connection));
}
