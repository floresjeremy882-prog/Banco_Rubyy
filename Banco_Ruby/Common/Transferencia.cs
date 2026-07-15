namespace BancoCenit.Common;

public sealed class Transferencia
{
    public string CuentaOrigen { get; init; }
    public string CuentaDestino { get; init; }
    public decimal Monto { get; init; }
    public DateTime Fecha { get; init; } = DateTime.UtcNow;

    public Transferencia(string origen, string destino, decimal monto)
    {
        CuentaOrigen = origen;
        CuentaDestino = destino;
        Monto = monto;
    }
}
