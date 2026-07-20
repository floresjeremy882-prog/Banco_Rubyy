using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BancoCenit.Infrastructure;
using BancoCenit.Common;
using BancoCenit.Features;
using BancoCenit.Domain.Transferencias;

namespace BancoCenit.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
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

        app.MapPost("/transferencia", async (TransferenciaRequest request, BancoRubyDbContext db, ITransferenciaGateway gateway) =>
            await TransferirSlice.TransferirAsync(request, db, gateway))
            .WithName("Transferir")
            .AddEndpointFilter<AccountAuthorizationFilter>();

        app.MapGet("/historial/{numeroCuenta}", HistorialSlice.ObtenerAsync)
            .WithName("Historial")
            .AddEndpointFilter<AccountAuthorizationFilter>();

        // Compatibility adapter for client `Usuario_Cliente` which calls /api/cuentas/{numero}/...
        app.MapPost("/api/cuentas/{numero}/autenticar", async (string numero, HttpRequest req, BancoRubyDbContext db) =>
        {
            Cuenta? cuenta = await db.Set<Cuenta>().Include(c => c.Usuario).AsNoTracking().FirstOrDefaultAsync(c => c.NumeroCuenta == numero && c.Estado);
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

        app.MapPost("/api/cuentas/{numeroOrigen}/transferir", async (string numeroOrigen, HttpRequest req, BancoRubyDbContext db, ITransferenciaGateway gateway) =>
        {
            try
            {
                using JsonDocument doc = await JsonDocument.ParseAsync(req.Body);
                JsonElement root = doc.RootElement;
                string cuentaDestino = root.GetProperty("CuentaDestino").GetString() ?? string.Empty;
                decimal monto = root.GetProperty("Monto").GetDecimal();

                return await TransferirSlice.TransferirAsync(new TransferenciaRequest(numeroOrigen, cuentaDestino, monto), db, gateway);
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

        return app;
    }
}
