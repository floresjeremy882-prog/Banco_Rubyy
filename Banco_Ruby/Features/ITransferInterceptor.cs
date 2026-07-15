using BancoCenit.Common;

namespace BancoCenit.Features;

public interface ITransferInterceptor
{
    Task InterceptTransferAsync(TransferenciaRequest request, Cuenta origen, Cuenta destino);
}
