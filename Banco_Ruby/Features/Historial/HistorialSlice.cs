using BancoCenit.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BancoCenit.Features;

public static class HistorialSlice
{
    public static async Task<object> ObtenerAsync(string numeroCuenta, BancoRubyDbContext db)
    {
        Cuenta cuenta = await db.Cuentas.FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta && c.Estado);
        if (cuenta is null)
        {
            return Results.NotFound(new { error = "Cuenta no encontrada o inactiva." });
        }

        List<HistorialResumen> auditorias = await db.Auditoria
            .AsNoTracking()
            .Where(a => a.CuentaId == cuenta.CuentaId)
            .OrderByDescending(a => a.CreadoEn)
            .Select(a => new HistorialResumen(a.Tipo, a.Monto, a.Descripcion, a.CreadoEn))
            .ToListAsync();

        return Results.Ok(new { titular = cuenta.Usuario?.Nombre, historial = auditorias });
    }

    private sealed record HistorialResumen(string Tipo, decimal Monto, string Descripcion, DateTime CreadoEn);
}
