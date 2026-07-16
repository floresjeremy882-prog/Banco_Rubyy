using BancoCenit;
using BancoCenit.Common;
using BancoCenit.Features;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BancoRubyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BancoRuby")));

builder.Services.AddScoped<AccountAuthorizationFilter>();

WebApplication app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "OK" }))
   .WithName("Health");

app.MapGet("/saldo/{numeroCuenta}", async (string numeroCuenta, BancoRubyDbContext db) =>
{
    return await AutenticacionSlice.ConsultarSaldoAsync(numeroCuenta, db);
})
.WithName("ConsultarSaldo")
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapPost("/deposito", DepositarSlice.DepositarAsync)
    .WithName("Depositar")
    .AddEndpointFilter<AccountAuthorizationFilter>();

app.MapPost("/retiro", RetirarSlice.RetirarAsync)
    .WithName("Retirar")
    .AddEndpointFilter<AccountAuthorizationFilter>();

app.MapPost("/transferencia", TransferirSlice.TransferirAsync)
    .WithName("Transferir")
    .AddEndpointFilter<AccountAuthorizationFilter>();

app.MapGet("/historial/{numeroCuenta}", HistorialSlice.ObtenerAsync)
    .WithName("Historial")
    .AddEndpointFilter<AccountAuthorizationFilter>();

// Compatibility adapter for client `Usuario_Cliente` which calls /api/cuentas/{numero}/...
app.MapPost("/api/cuentas/{numero}/autenticar", async (string numero, HttpRequest req, BancoRubyDbContext db) =>
{
    Cuenta? cuenta = await db.Cuentas.Include(c => c.Usuario).AsNoTracking().FirstOrDefaultAsync(c => c.NumeroCuenta == numero && c.Estado);
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

app.MapGet("/api/cuentas/{numero}/saldo", async (string numero, BancoRubyDbContext db) =>
{
    return await AutenticacionSlice.ConsultarSaldoAsync(numero, db);
})
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapPost("/api/cuentas/{numero}/depositar", async (string numero, HttpRequest req, BancoRubyDbContext db) =>
{
    try
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(req.Body);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("Monto", out JsonElement m) && !root.TryGetProperty("monto", out m))
        {
            return Results.BadRequest(new { error = "Cuerpo inválido" });
        }

        decimal monto = m.GetDecimal();
        return await DepositarSlice.DepositarAsync(new DepositoRequest(numero, monto), db);
    }
    catch
    {
        return Results.BadRequest(new { error = "Cuerpo inválido" });
    }
})
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapPost("/api/cuentas/{numero}/retirar", async (string numero, HttpRequest req, BancoRubyDbContext db) =>
{
    try
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(req.Body);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("Monto", out JsonElement m) && !root.TryGetProperty("monto", out m))
        {
            return Results.BadRequest(new { error = "Cuerpo inválido" });
        }

        decimal monto = m.GetDecimal();
        return await RetirarSlice.RetirarAsync(new RetiroRequest(numero, monto), db);
    }
    catch
    {
        return Results.BadRequest(new { error = "Cuerpo inválido" });
    }
})
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapPost("/api/cuentas/{numeroOrigen}/transferir", async (string numeroOrigen, HttpRequest req, BancoRubyDbContext db) =>
{
    try
    {
        using JsonDocument doc = await JsonDocument.ParseAsync(req.Body);
        JsonElement root = doc.RootElement;
        string cuentaDestino = root.GetProperty("CuentaDestino").GetString() ?? string.Empty;
        decimal monto = root.GetProperty("Monto").GetDecimal();

        return await TransferirSlice.TransferirAsync(new TransferenciaRequest(numeroOrigen, cuentaDestino, monto), db);
    }
    catch
    {
        return Results.BadRequest(new { error = "Cuerpo inválido para transferencia." });
    }
})
.AddEndpointFilter<AccountAuthorizationFilter>();

app.MapGet("/api/cuentas/{numero}/historial", async (string numero, BancoRubyDbContext db) =>
{
    return await HistorialSlice.ObtenerAsync(numero, db);
})
.AddEndpointFilter<AccountAuthorizationFilter>();

app.Run();
