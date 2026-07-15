using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace BancoCenit.Common;

public interface IDomainEvent { }

public sealed class AuditoriaEvent : IDomainEvent
{
    public int CuentaId { get; init; }
    public string NumeroCuenta { get; init; } = default!;
    public string Tipo { get; init; } = default!;
    public decimal Monto { get; init; }
    public string Descripcion { get; init; } = default!;
    public DateTime CreadoEn { get; init; }
}

public sealed class PagoCompletadoEvent : IDomainEvent
{
    public int CuentaId { get; init; }
    public string NumeroCuenta { get; init; } = default!;
    public string Tipo { get; init; } = "PagoCompletado";
    public decimal Monto { get; init; }
    public string Descripcion { get; init; } = default!;
    public DateTime CreadoEn { get; init; }
}

public interface IEventBus
{
    ValueTask PublishAsync(IDomainEvent domainEvent);
}

public sealed class InMemoryEventBus : BackgroundService, IEventBus
{
    private readonly Channel<IDomainEvent> _channel = Channel.CreateBounded<IDomainEvent>(new BoundedChannelOptions(1000)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.Wait
    });

    private readonly IServiceProvider _serviceProvider;

    public InMemoryEventBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ValueTask PublishAsync(IDomainEvent domainEvent)
    {
        return _channel.Writer.WriteAsync(domainEvent);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (IDomainEvent domainEvent in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                BancoRubyDbContext db = scope.ServiceProvider.GetRequiredService<BancoRubyDbContext>();
                await HandleEventAsync(domainEvent, db, stoppingToken);
            }
            catch
            {
                // El evento se descarta si falla el manejo; mantener el servicio estable.
            }
        }
    }

    private static async Task HandleEventAsync(IDomainEvent domainEvent, BancoRubyDbContext db, CancellationToken cancellationToken)
    {
        switch (domainEvent)
        {
            case AuditoriaEvent audit:
                db.Auditoria.Add(new Auditoria
                {
                    CuentaId = audit.CuentaId,
                    NumeroCuenta = audit.NumeroCuenta,
                    Tipo = audit.Tipo,
                    Monto = audit.Monto,
                    Descripcion = audit.Descripcion,
                    CreadoEn = audit.CreadoEn
                });
                await db.SaveChangesAsync(cancellationToken);
                break;

            case PagoCompletadoEvent pago:
                db.Auditoria.Add(new Auditoria
                {
                    CuentaId = pago.CuentaId,
                    NumeroCuenta = pago.NumeroCuenta,
                    Tipo = pago.Tipo,
                    Monto = pago.Monto,
                    Descripcion = pago.Descripcion,
                    CreadoEn = pago.CreadoEn
                });
                await db.SaveChangesAsync(cancellationToken);
                break;
        }
    }
}
