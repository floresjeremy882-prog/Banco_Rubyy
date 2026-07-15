using BancoCenit;
using BancoCenit.Common;
using System.Text.Json;
using BancoCenit.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BancoRubyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BancoRuby")));

builder.Services.AddSingleton<InMemoryEventBus>();
builder.Services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<InMemoryEventBus>());
builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<InMemoryEventBus>());

builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddScoped<ITransferInterceptor, DefaultTransferInterceptor>();
builder.Services.AddScoped<AccountAuthorizationFilter>();

WebApplication app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "OK" }))
   .WithName("Health");

static IResult FromResult<T>(OperationResult<T> result)
{
    return result.IsSuccess
        ? Results.Ok(result.Value!) : Results.Problem(statusCode: result.StatusCode, title: result.Error);
}

app.MapGet("/saldo/{numeroCuenta}", async (string numeroCuenta, IBankService bankService) =>
{
    return FromResult(await bankService.ConsultarSaldoAsync(numeroCuenta));
})
.WithName("ConsultarSaldo")
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapPost("/deposito", async (DepositoRequest request, IBankService bankService) =>
{
    return FromResult(await bankService.DepositarAsync(request));
})
.WithName("Depositar")
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapPost("/retiro", async (RetiroRequest request, IBankService bankService) =>
{
    return FromResult(await bankService.RetirarAsync(request));
})
.WithName("Retirar")
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapPost("/transferencia", async (TransferenciaRequest request, IBankService bankService) =>
{
    return FromResult(await bankService.TransferirAsync(request));
})
.WithName("Transferir")
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapGet("/historial/{numeroCuenta}", async (string numeroCuenta, IBankService bankService) =>
{
    return FromResult(await bankService.ObtenerHistorialAsync(numeroCuenta));
})
.WithName("Historial")
.AddEndpointFilter<AccountAuthorizationFilter>();

// Compatibility adapter for client `Usuario_Cliente` which calls /api/cuentas/{numero}/...
app.MapPost("/api/cuentas/{numero}/autenticar", async (string numero, HttpRequest req, BancoRubyDbContext db) =>
{
    Cuenta cuenta = await db.Cuentas.Include(c => c.Usuario).AsNoTracking().FirstOrDefaultAsync(c => c.NumeroCuenta == numero && c.Estado);
    if (cuenta is null) return Results.NotFound(new { error = "Cuenta no encontrada o inactiva." });

    try
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(req.Body);
    }
    catch
    {
    }

    return Results.Ok(new { titular = cuenta.Usuario?.Nombre, cuenta = cuenta.NumeroCuenta });
});

app.MapGet("/api/cuentas/{numero}/saldo", async (string numero, IBankService bankService) =>
{
    return FromResult(await bankService.ConsultarSaldoAsync(numero));
})
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapPost("/api/cuentas/{numero}/depositar", async (string numero, HttpRequest req, IBankService bankService) =>
{
    decimal monto = 0;
    try
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(req.Body);
        if (doc.RootElement.TryGetProperty("Monto", out JsonElement m)) monto = m.GetDecimal();
    }
    catch
    {
        return Results.BadRequest(new { error = "Cuerpo inválido" });
    }

    return FromResult(await bankService.DepositarAsync(new DepositoRequest(numero, monto)));
})
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapPost("/api/cuentas/{numero}/retirar", async (string numero, HttpRequest req, IBankService bankService) =>
{
    decimal monto = 0;
    try
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(req.Body);
        if (doc.RootElement.TryGetProperty("Monto", out JsonElement m)) monto = m.GetDecimal();
    }
    catch
    {
        return Results.BadRequest(new { error = "Cuerpo inválido" });
    }

    return FromResult(await bankService.RetirarAsync(new RetiroRequest(numero, monto)));
})
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapPost("/api/cuentas/{numeroOrigen}/transferir", async (string numeroOrigen, HttpRequest req, IBankService bankService) =>
{
    try
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(req.Body);
        JsonElement root = doc.RootElement;
        string cuentaDestino = root.GetProperty("CuentaDestino").GetString() ?? string.Empty;
        decimal monto = root.GetProperty("Monto").GetDecimal();

        return FromResult(await bankService.TransferirAsync(new TransferenciaRequest(numeroOrigen, cuentaDestino, monto)));
    }
    catch
    {
        return Results.BadRequest(new { error = "Cuerpo inválido para transferencia." });
    }
})
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapGet("/api/cuentas/{numero}/historial", async (string numero, IBankService bankService) =>
{
    return FromResult(await bankService.ObtenerHistorialAsync(numero));
})
.AddEndpointFilter<AccountAuthorizationFilter>();

app.Run();
