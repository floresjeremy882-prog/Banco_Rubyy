namespace BancoCenit.Common;

public sealed record DepositoRequest(string NumeroCuenta, decimal Monto);
public sealed record RetiroRequest(string NumeroCuenta, decimal Monto);
public sealed record TransferenciaRequest(string NumeroCuentaOrigen, string NumeroCuentaDestino, decimal Monto);
