using BancoCenit.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BancoCenit.Features;

public static class RetirarSlice
{
    public static async Task<object> RetirarAsync(RetiroRequest request, BancoRubyDbContext db)
    {
        Cuenta cuenta = await db.Cuentas.FirstOrDefaultAsync(c => c.NumeroCuenta == request.NumeroCuenta && c.Estado);
        if (cuenta is null)
        {
            return Results.NotFound(new { error = "Cuenta no encontrada o inactiva." });
        }

        if (request.Monto <= 0)
        {
            return Results.BadRequest(new { error = "El monto debe ser mayor que cero." });
        }

        if (request.Monto % 10 != 0)
        {
            return Results.BadRequest(new { error = "El retiro debe ser múltiplo de 10." });
        }

        if (request.Monto > 500)
        {
            return Results.BadRequest(new { error = "El retiro excede el límite de 500." });
        }

        if (request.Monto > cuenta.Saldo)
        {
            return Results.BadRequest(new { error = "Fondos insuficientes." });
        }

        cuenta.Saldo -= request.Monto;
        db.Auditoria.Add(new Auditoria
        {
            CuentaId = cuenta.CuentaId,
            NumeroCuenta = cuenta.NumeroCuenta,
            Tipo = "Retiro",
            Monto = request.Monto,
            Descripcion = $"Retiro de {request.Monto:N2}",
            CreadoEn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return Results.Ok(new { mensaje = $"Retiro de ${request.Monto:N2} realizado.", saldo = cuenta.Saldo });
    }
}
