using BancoCenit.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BancoCenit.Features;

public static class TransferirSlice
{
    public static async Task<object> TransferirAsync(TransferenciaRequest request, BancoRubyDbContext db)
    {
        if (request.NumeroCuentaOrigen == request.NumeroCuentaDestino)
        {
            return Results.BadRequest(new { error = "La cuenta origen y destino no pueden ser la misma." });
        }

        Cuenta? origen = await db.Cuentas.FirstOrDefaultAsync(c => c.NumeroCuenta == request.NumeroCuentaOrigen && c.Estado);
        if (origen is null)
        {
            return Results.NotFound(new { error = "Cuenta origen no encontrada o inactiva." });
        }

        Cuenta? destino = await db.Cuentas.FirstOrDefaultAsync(c => c.NumeroCuenta == request.NumeroCuentaDestino && c.Estado);
        if (destino is null)
        {
            return Results.NotFound(new { error = "Cuenta destino no encontrada o inactiva." });
        }

        if (request.Monto <= 0)
        {
            return Results.BadRequest(new { error = "El monto debe ser mayor que cero." });
        }

        if (request.Monto > origen.Saldo)
        {
            return Results.BadRequest(new { error = "Fondos insuficientes en la cuenta origen." });
        }

        origen.Saldo -= request.Monto;
        destino.Saldo += request.Monto;

        db.Auditoria.Add(new Auditoria
        {
            CuentaId = origen.CuentaId,
            NumeroCuenta = origen.NumeroCuenta,
            Tipo = "Transferencia enviada",
            Monto = request.Monto,
            Descripcion = $"Se envió transferencia de ${request.Monto:N2} a la cuenta {destino.NumeroCuenta}.",
            CreadoEn = DateTime.UtcNow
        });

        db.Auditoria.Add(new Auditoria
        {
            CuentaId = destino.CuentaId,
            NumeroCuenta = destino.NumeroCuenta,
            Tipo = "Transferencia recibida",
            Monto = request.Monto,
            Descripcion = $"Se recibió transferencia de la cuenta {origen.NumeroCuenta} por ${request.Monto:N2}.",
            CreadoEn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            mensaje = $"Transferencia de ${request.Monto:N2} realizada de {origen.NumeroCuenta} a {destino.NumeroCuenta}.",
            saldoOrigen = origen.Saldo,
            saldoDestino = destino.Saldo
        });
    }
}
