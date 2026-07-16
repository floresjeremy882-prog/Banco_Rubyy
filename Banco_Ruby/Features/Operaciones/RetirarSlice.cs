using BancoCenit.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BancoCenit.Features;

public static class RetirarSlice
{
    private const decimal COMISION = 0.41m;

    public static async Task<object> RetirarAsync(RetiroRequest request, BancoRubyDbContext db)
    {
        Cuenta? cuenta = await db.Cuentas.FirstOrDefaultAsync(c => c.NumeroCuenta == request.NumeroCuenta && c.Estado);
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

        decimal totalDebitado = request.Monto + COMISION;
        if (totalDebitado > cuenta.Saldo)
        {
            return Results.BadRequest(new { error = "Fondos insuficientes." });
        }

        cuenta.Saldo -= totalDebitado;
        db.Auditoria.Add(new Auditoria
        {
            CuentaId = cuenta.CuentaId,
            NumeroCuenta = cuenta.NumeroCuenta,
            Tipo = "Retiro",
            Monto = totalDebitado,
            Descripcion = $"Se debitó de la cuenta ${request.Monto:N2} más comisión de ${COMISION:N2}.",
            CreadoEn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return Results.Ok(new { mensaje = $"Retiro de ${request.Monto:N2} realizado con comisión de ${COMISION:N2}.", saldo = cuenta.Saldo });
    }
}
