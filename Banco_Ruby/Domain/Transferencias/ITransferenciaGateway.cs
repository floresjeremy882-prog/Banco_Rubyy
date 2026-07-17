namespace BancoCenit.Domain.Transferencias;

public interface ITransferenciaGateway
{
    Task EnviarAsync(string cuentaOrigen, string cuentaDestino, decimal monto, CancellationToken cancellationToken = default);
}
