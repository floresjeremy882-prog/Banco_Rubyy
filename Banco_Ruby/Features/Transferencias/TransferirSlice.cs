using BancoCenit.Common;
using BancoCenit.Domain.Transferencias;
using BancoCenit.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BancoCenit.Features;

public static class TransferirSlice
{
    public static async Task<object> TransferirAsync(TransferenciaRequest request, DbContext db, ITransferenciaGateway gateway)
    {
        if (request.NumeroCuentaOrigen == request.NumeroCuentaDestino)
        {
            return Results.BadRequest(new { error = "La cuenta origen y destino no pueden ser la misma." });
        }

        Cuenta? origen = await db.Set<Cuenta>().FirstOrDefaultAsync(c => c.NumeroCuenta == request.NumeroCuentaOrigen && c.Estado);
        if (origen is null)
        {
            return Results.NotFound(new { error = "Cuenta origen no encontrada o inactiva." });
        }

        Cuenta? destino = await db.Set<Cuenta>().FirstOrDefaultAsync(c => c.NumeroCuenta == request.NumeroCuentaDestino && c.Estado);
        if (destino is null)
        {
            return Results.NotFound(new { error = "Cuenta destino no encontrada o inactiva." });
        }

        TransferenciaExecutionResult resultado = await TransferenciaService.EjecutarTransferenciaAsync(
            origen,
            destino,
            request,
            () => gateway.EnviarAsync(origen.NumeroCuenta, destino.NumeroCuenta, request.Monto));

        if (!resultado.IsSuccess)
        {
            return Results.BadRequest(new { error = resultado.Error });
        }

        db.Set<Auditoria>().Add(new Auditoria
        {
            CuentaId = origen.CuentaId,
            NumeroCuenta = origen.NumeroCuenta,
            Tipo = "Transferencia enviada",
            Monto = request.Monto,
            Descripcion = $"Se envió transferencia de ${request.Monto:N2} a la cuenta {destino.NumeroCuenta}.",
            CreadoEn = DateTime.UtcNow
        });

        db.Set<Auditoria>().Add(new Auditoria
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
