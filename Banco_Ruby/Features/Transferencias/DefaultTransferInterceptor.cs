using BancoCenit.Common;

namespace BancoCenit.Features;

public sealed class DefaultTransferInterceptor : ITransferInterceptor
{
    public Task InterceptTransferAsync(TransferenciaRequest request, Cuenta origen, Cuenta destino)
    {
        // Interceptor base que no cambia el comportamiento, solo sirve como placeholder.
        return Task.CompletedTask;
    }
}
