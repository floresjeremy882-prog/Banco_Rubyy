using BancoCenit.Common;

namespace BancoCenit.Domain.Transferencias;

public sealed class TransferenciaExecutionResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private TransferenciaExecutionResult(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static TransferenciaExecutionResult Success() => new(true, null);
    public static TransferenciaExecutionResult Failure(string error) => new(false, error);
}

public static class TransferenciaService
{
    public static async Task<TransferenciaExecutionResult> EjecutarTransferenciaAsync(
        Cuenta origen,
        Cuenta destino,
        TransferenciaRequest request,
        Func<Task> enviarTransferencia)
    {
        if (request.Monto <= 0)
        {
            return TransferenciaExecutionResult.Failure("El monto debe ser mayor que cero.");
        }

        if (request.Monto > origen.Saldo)
        {
            return TransferenciaExecutionResult.Failure("Fondos insuficientes en la cuenta origen.");
        }

        decimal saldoOrigenAntes = origen.Saldo;
        decimal saldoDestinoAntes = destino.Saldo;

        origen.Saldo -= request.Monto;
        destino.Saldo += request.Monto;

        try
        {
            await enviarTransferencia();
            return TransferenciaExecutionResult.Success();
        }
        catch (Exception ex)
        {
            origen.Saldo = saldoOrigenAntes;
            destino.Saldo = saldoDestinoAntes;
            return TransferenciaExecutionResult.Failure($"Transacción fallida. Se devolvió el monto a la cuenta {origen.NumeroCuenta}. Detalle: {ex.Message}");
        }
    }
}
