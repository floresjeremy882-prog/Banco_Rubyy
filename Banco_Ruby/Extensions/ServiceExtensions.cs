using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BancoCenit.Infrastructure;
using BancoCenit.Features;
using BancoCenit.Domain.Transferencias;

namespace BancoCenit.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BancoRubyDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("BancoRuby")));

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<BancoRubyDbContext>());

        services.AddScoped<AccountAuthorizationFilter>();
        services.AddScoped<ITransferenciaGateway, TransferenciaGateway>();

        return services;
    }
}
