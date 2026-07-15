namespace BancoCenit.Features;

public sealed record SaldoResponse(decimal Saldo, string Titular);
public sealed record OperacionResponse(string Mensaje, decimal Saldo);
public sealed record TransferenciaResponse(string Mensaje, decimal SaldoOrigen, decimal SaldoDestino);
public sealed record HistorialItem(string Tipo, decimal Monto, string Descripcion, DateTime CreadoEn);
public sealed record HistorialResponse(string Titular, IReadOnlyCollection<HistorialItem> Historial);
