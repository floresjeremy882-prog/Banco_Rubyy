using BancoCenit.Common;
using BancoCenit.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BancoCenit.Features;

public static class HistorialSlice
{
    public static async Task<object> ObtenerAsync(string numeroCuenta, DbContext db)
    {
        Cuenta? cuenta = await db.Set<Cuenta>()
            .Include(c => c.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta && c.Estado);
        if (cuenta is null)
        {
            return Results.NotFound(new { error = "Cuenta no encontrada o inactiva." });
        }

        List<HistorialResumen> auditorias = await db.Set<Auditoria>()
            .AsNoTracking()
            .Where(a => a.CuentaId == cuenta.CuentaId)
            .OrderByDescending(a => a.CreadoEn)
            .Select(a => new HistorialResumen(a.Tipo, a.Monto, a.Descripcion, a.CreadoEn))
            .ToListAsync();

        return Results.Ok(new { titular = cuenta.Usuario?.Nombre ?? string.Empty, historial = auditorias });
    }

    private sealed record HistorialResumen(string Tipo, decimal Monto, string Descripcion, DateTime CreadoEn);
}
