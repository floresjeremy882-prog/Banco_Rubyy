using BancoCenit.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;

namespace BancoCenit.Features;

public sealed class AccountAuthorizationFilter : IEndpointFilter
{
    private readonly BancoRubyDbContext _db;

    public AccountAuthorizationFilter(BancoRubyDbContext db)
    {
        _db = db;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var accountNumbers = GetAccountNumbers(context.Arguments);
        if (accountNumbers.Count > 0)
        {
            foreach (string numeroCuenta in accountNumbers.Distinct())
            {
                bool exists = await _db.Cuentas.AsNoTracking().AnyAsync(c => c.NumeroCuenta == numeroCuenta && c.Estado);
                if (!exists)
                {
                    return Results.NotFound(new { error = $"Cuenta {numeroCuenta} no encontrada o inactiva." });
                }
            }
        }

        return await next(context);
    }

    private static IReadOnlyCollection<string> GetAccountNumbers(IList<object?> arguments)
    {
        var accountNumbers = new List<string>();

        foreach (object? arg in arguments)
        {
            switch (arg)
            {
                case string value when !string.IsNullOrWhiteSpace(value):
                    accountNumbers.Add(value);
                    break;
                case DepositoRequest request:
                    accountNumbers.Add(request.NumeroCuenta);
                    break;
                case RetiroRequest request:
                    accountNumbers.Add(request.NumeroCuenta);
                    break;
                case TransferenciaRequest request:
                    accountNumbers.Add(request.NumeroCuentaOrigen);
                    accountNumbers.Add(request.NumeroCuentaDestino);
                    break;
            }
        }

        return accountNumbers;
    }
}
