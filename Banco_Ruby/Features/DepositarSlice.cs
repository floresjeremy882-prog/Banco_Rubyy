using BancoCenit.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BancoCenit.Features;

public static class DepositarSlice
{
    public static async Task<object> DepositarAsync(DepositoRequest request, BancoRubyDbContext db)
    {
        Cuenta cuenta = await db.Cuentas
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.NumeroCuenta == request.NumeroCuenta && c.Estado);

        if (cuenta is null)
        {
            return Results.NotFound(new { error = "Cuenta no encontrada o inactiva." });
        }

        if (request.Monto <= 0)
        {
            return Results.BadRequest(new { error = "El monto debe ser mayor que cero." });
        }

        cuenta.Saldo += request.Monto;
        db.Auditoria.Add(new Auditoria
        {
            CuentaId = cuenta.CuentaId,
            NumeroCuenta = cuenta.NumeroCuenta,
            Tipo = "Depósito",
            Monto = request.Monto,
            Descripcion = $"Depósito de {request.Monto:N2}",
            CreadoEn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return Results.Ok(new { mensaje = $"Depósito de ${request.Monto:N2} realizado.", saldo = cuenta.Saldo });
    }
}
