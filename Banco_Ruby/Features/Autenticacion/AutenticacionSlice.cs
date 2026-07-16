using BancoCenit.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BancoCenit.Features;

public static class AutenticacionSlice
{
    public static async Task<object> ConsultarSaldoAsync(string numeroCuenta, BancoRubyDbContext db)
    {
        Cuenta? cuenta = await db.Cuentas
            .Include(c => c.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta && c.Estado);

        return cuenta is null
            ? Results.NotFound(new { error = "Cuenta no encontrada o inactiva." })
            : Results.Ok(new { saldo = cuenta.Saldo, titular = cuenta.Usuario?.Nombre ?? string.Empty });
    }
}
