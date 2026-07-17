using BancoCenit.Domain.Transferencias;

namespace BancoCenit.Infrastructure;

public sealed class TransferenciaGateway : ITransferenciaGateway
{
    public Task EnviarAsync(string cuentaOrigen, string cuentaDestino, decimal monto, CancellationToken cancellationToken = default)
    {
        // Simulación de un envío externo. En un escenario real, aquí iría la llamada HTTP/REST o al banco destino.
        return Task.CompletedTask;
    }
}
